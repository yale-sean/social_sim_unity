using UnityEngine;
using RosSharp.RosBridgeClient;

public class CountCollisions : MonoBehaviour
{
    public TrialStatusPublisher trialSystem;

    private void OnTriggerEnter(Collider hit)
    {
    	if (hit.isTrigger)
    		return;
    	
        if (hit.gameObject.tag == "Actor")
            trialSystem.IncrementPeopleCollisions();
        else
            trialSystem.IncrementObjectCollisions();
        //Debug.Log("People Collisions: " + trialSystem.GetPeopleCollisions()
        //        + "    Object Collisions: " + trialSystem.GetObjectCollisions());
    }
}
