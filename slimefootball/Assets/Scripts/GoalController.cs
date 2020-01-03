using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalController : MonoBehaviour
{
    public delegate void OnGoalEvent_Delegate(GameObject gameObject);
    public event OnGoalEvent_Delegate OnGoalEvent;

    private void OnTriggerEnter2D( Collider2D collision )
    {
        if(collision.gameObject.tag == Common.ballTag)
        {
            OnGoalEvent(gameObject);
        }
    }
}
