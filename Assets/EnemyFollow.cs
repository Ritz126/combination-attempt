using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    //I first start getting the transform (position) of my player
    public Transform target;
    //Then I set up the speed of the enemy, that I can edit later
    public float speed = 2f;
    //Lastly, I added the enemy a rigidbody
    public Rigidbody rb;

    //First thing, I will create a function that follows the player
    void FollowPlayer()
    {
        //I will create a vector 3 called pos that stores the movement that I want my player to do
        Vector3 pos = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        //I will use these two built-in functions to follow the player
        rb.MovePosition(pos);
        transform.LookAt(target);
    }

    //Finally, I add a collider function that calls the FollowPlayer() function when it is within its range
    void OnTriggerStay(Collider player)
    {
        if (player.tag == "Player")
        {
            FollowPlayer();
        }
    }
}
