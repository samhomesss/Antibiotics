using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMoveTrigger : MonoBehaviour
{
    public bool isEnemyMove = false;

    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isEnemyMove = true;
        }
    }
}
