using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCreamController : MonoBehaviour
{
    [SerializeField] GameObject iceCreamsParent;
    public List<ProceduralCircle> iceCreams;

    public int lastActiveCircle = 0;
    public int lastUpdatingFlavor = -1;

    public IceCreamState state;

    void Start()
    {
        foreach(var iceCream in iceCreamsParent.GetComponentsInChildren<ProceduralCircle>())
        {
            iceCreams.Add(iceCream);
        }
    }

    public void SetFlavorActive(int val)
    {
        for(int i=0; i<3; i++)
        {
            if (iceCreams[i].IsEmitting())
                iceCreams[i].FlipEmit();
        }
        iceCreams[val].FlipEmit();
    }

    void Update()
    {
        for(int i=0; i<iceCreams.Count; i++)
        {
            switch(state)
            {
                case IceCreamState.Create:
                    //iceCreams[i].UpdateIceCream();

                    break;
                case IceCreamState.Simulate:
                    if (iceCreams[i].iceCreamState== IceCreamState.Create)
                    {
                        iceCreams[i].iceCreamState = IceCreamState.Simulate;
                        iceCreams[i].ResetPosition();
                    }
                    if (lastUpdatingFlavor==i)
                    {
                        //iceCreams[i].UpdateIceCream();
                        lastActiveCircle = Mathf.Max(lastActiveCircle, iceCreams[i].GetLastActiveCircle());
                        lastUpdatingFlavor= i;
                    }
                    break;
            }
        }
    }
}
