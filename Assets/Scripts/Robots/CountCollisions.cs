using UnityEngine;
using RosSharp.RosBridgeClient;

public class CountCollisions : MonoBehaviour
{
    public TrialStatusPublisher trial;

    private void OnTriggerEnter(Collider hit)
    {
    	if (hit.isTrigger)
    		return;
    	
        if (hit.gameObject.tag == "Actor")
            trial.IncrementPeopleCollisions();
        else
            trial.IncrementObjectCollisions();
        Debug.Log("People Collisions: " + trial.GetPeopleCollisions() 
        	+ "    Object Collisions: " + trial.GetObjectCollisions());
    }
}
