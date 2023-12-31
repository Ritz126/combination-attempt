using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[CreateAssetMenu(fileName = "Gun", menuName = "Guns/Gun", order = 0)]
public class GunScriptableObject : ScriptableObject
{
    public GunType Type;
    public string Name;
    public GameObject ModelPrefab;
    public Vector3 SpawnPoint;
    public Vector3 SpawnRotation;

    public DamageConfigScriptableObject DamageConfig;
    public ShootConfigScriptableObject ShootConfig;
    public TrailConfigScriptableObject TrailConfig;

    private MonoBehaviour ActiveMonoBehaviour;
    private GameObject Model;
    private float LastShootTime;
    private float InitialClickTime;
    private float StopShootingTime;
    private bool LastFrameWantedToShoot;
    private ParticleSystem ShootSystem;
    private ObjectPool<TrailRenderer> TrailPool;

    public void Spawn(Transform Parent, MonoBehaviour ActiveMonoBehaviour)
    {
        this.ActiveMonoBehaviour = ActiveMonoBehaviour;
        LastShootTime = 0; // in editor this will be resest, in build no problemo
        TrailPool = new ObjectPool<TrailRenderer>(CreateTrail);

        Model = Instantiate(ModelPrefab);
        Model.transform.SetParent(Parent, false);
        Model.transform.localPosition = SpawnPoint;
        Model.transform.localRotation = Quaternion.Euler(SpawnRotation);

        ShootSystem = Model.GetComponentInChildren<ParticleSystem>();
    }
    public void Shoot()
    {
        if (Time.time - LastShootTime - ShootConfig.FireRate > Time.deltaTime)
        {
            float lastDuration = Mathf.Clamp(
                0,
                (StopShootingTime - InitialClickTime),
                ShootConfig.MaxSpreadTime
            );
            float lerpTime = (ShootConfig.RecoilRecoverySpeed - (Time.time - StopShootingTime))
                             / ShootConfig.RecoilRecoverySpeed;

            InitialClickTime = Time.time - Mathf.Lerp(0, lastDuration, Mathf.Clamp01(lerpTime));

        }

        if (Time.time > ShootConfig.FireRate + LastShootTime)
        {
            LastShootTime = Time.time;
            ShootSystem.Play();

            Vector3 spreadAmount = ShootConfig.GetSpread(Time.time - InitialClickTime);                 
            Model.transform.forward += Model.transform.TransformDirection(spreadAmount);

            Vector3 shootDirection = Model.transform.forward;

            if(Physics.Raycast(
                    ShootSystem.transform.position,
                    shootDirection,
                    out RaycastHit hit,
                    float.MaxValue,
                    ShootConfig.HitMask
                ))
            {
                ActiveMonoBehaviour.StartCoroutine(
                    PlayTrail(
                        ShootSystem.transform.position,
                        hit.point,
                        hit
                    )
                );
            }
            else
            {
                ActiveMonoBehaviour.StartCoroutine(
                    PlayTrail(
                        ShootSystem.transform.position,
                        ShootSystem.transform.position + (shootDirection * TrailConfig.MissDistance),
                        new RaycastHit()
                    )
                );
            }
        }
    }

    public void Tick(bool WantsToShoot)
    {
        Model.transform.localRotation = Quaternion.Lerp(
            Model.transform.localRotation,
            Quaternion.Euler(SpawnRotation),
            Time.deltaTime * ShootConfig.RecoilRecoverySpeed
            );
        if (WantsToShoot)
        {
            LastFrameWantedToShoot = true;
            Shoot();
        }

        else if (!WantsToShoot && LastFrameWantedToShoot)
        {
            StopShootingTime = Time.time;
            LastFrameWantedToShoot = false;
        }
    }

    private IEnumerator PlayTrail (Vector3 StartPoint, Vector3 EndPoint, RaycastHit Hit)
    {
        TrailRenderer instance = TrailPool.Get();
        instance.gameObject.SetActive(true);
        instance.transform.position = StartPoint;
        yield return null; // for no position carry over on last frame 
       
        instance.emitting = true;

        float distance = Vector3.Distance(StartPoint, EndPoint);
        float remainingDistance = distance;
        while (remainingDistance > 0)
        {
            instance.transform.position = Vector3.Lerp(
                StartPoint,
                EndPoint,
                Mathf.Clamp01(1 - (remainingDistance / distance))
            );
            remainingDistance -= TrailConfig.SimulationSpeed * Time.deltaTime;

            yield return null;
        }

        instance.transform.position = EndPoint;

        if (Hit.collider != null)
        {
            if (Hit.collider.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(DamageConfig.GetDamage(distance));
            }
        }

        yield return new WaitForSeconds(TrailConfig.Duration);
        yield return null;
        instance.emitting = false;
        instance.gameObject.SetActive(false);
        TrailPool.Release(instance);
    }
    private TrailRenderer CreateTrail()
    {
        GameObject instance = new GameObject("Bullet Trail");
        TrailRenderer trail = instance.AddComponent<TrailRenderer>();
        trail.colorGradient = TrailConfig.Color;
        trail.material = TrailConfig.Material;
        trail.widthCurve = TrailConfig.WidthCurve;
        trail.time = TrailConfig.Duration;
        trail.minVertexDistance = TrailConfig.MinVertexDistance;

        trail.emitting = false;
        trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return trail;
    }
}
