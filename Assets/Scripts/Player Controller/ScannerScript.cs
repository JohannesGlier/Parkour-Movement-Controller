using System.Collections.Generic;
using UnityEngine;

public class ScannerScript : Singleton<ScannerScript>
{
    [SerializeField] string[] enemyTags;
    [SerializeField] List<GameObject> enemyList = new List<GameObject>();


    public List<GameObject> EnemyList => enemyList;
    public string[] EnemyTags => enemyTags;


    public void OnTriggerEnter(Collider other)
    {
        foreach(string enemyTag in enemyTags)
        {
            if (other.tag == enemyTag)
                enemyList.Add(other.gameObject);
        }  
    }

    public void OnTriggerStay(Collider other)
    {
        foreach (string enemyTag in enemyTags)
        {
            if (other.tag == enemyTag && !enemyList.Contains(other.gameObject))
                enemyList.Add(other.gameObject);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        foreach (string enemyTag in enemyTags)
        {
            if (other.tag == enemyTag)
                enemyList.Remove(other.gameObject);
        }
    }
}
