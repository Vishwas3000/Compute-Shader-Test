using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static IceCreamController;

[CustomEditor(typeof(IceCreamController))]
public class IceCreamConroller_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        IceCreamController controller = (IceCreamController)target;

        if (GUILayout.Button("Emit colol 1"))
        {
            controller.iceCreams[0].SetStartActiveCircle(controller.lastActiveCircle);
            controller.lastUpdatingFlavor = 0;
            controller.iceCreams[0].FlipEmit();
        }
        if (GUILayout.Button("Emit colol 2"))
        {
            controller.iceCreams[1].SetStartActiveCircle(controller.lastActiveCircle);
            controller.lastUpdatingFlavor = 1;
            controller.iceCreams[1].FlipEmit();
        }
        if (GUILayout.Button("Emit colol 3"))
        {
            controller.iceCreams[2].SetStartActiveCircle(controller.lastActiveCircle);
            controller.lastUpdatingFlavor = 2;
            controller.iceCreams[2].FlipEmit();
        }

    }
}
