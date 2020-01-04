using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    public delegate void TriggerEnter_Delegate( Collider2D collision );
    public event TriggerEnter_Delegate OnTriggerEnterEvent = delegate{};

    public delegate void TriggerStay_Delegate( Collider2D collision );
    public event TriggerStay_Delegate OnTriggerStayEvent = delegate{};

    public delegate void TriggerExit_Delegate( Collider2D collision );
    public event TriggerExit_Delegate OnTriggerExitEvent = delegate{};

    private void OnTriggerEnter2D( Collider2D collision )
    {
        OnTriggerEnterEvent( collision );
    }

    private void OnTriggerStay2D( Collider2D collision )
    {
        OnTriggerStayEvent( collision );
    }

    private void OnTriggerExit2D( Collider2D collision )
    {
        OnTriggerExitEvent( collision );
    }
}
