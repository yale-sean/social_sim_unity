using UnityEngine;
using UnityEngine.AI;

namespace IVI
{
    public class DummyAgent : INavigable
    {

        protected override bool PlanNavigation()
        {
            var nma = GetComponent<NavMeshAgent>();
            nma.isStopped = false;
            nma.SetDestination(destPos);
            return true;
        }

        protected override void StopNavigation()
        {
            GetComponent<NavMeshAgent>().isStopped = true;
        }

        protected override void StartGroup(GroupNavNode group)
        {
            group.AddMember(this);
        }

        protected override void StopGroup(GroupNavNode group)
        {
            group.RemoveMember(this);
        }

    }
}
