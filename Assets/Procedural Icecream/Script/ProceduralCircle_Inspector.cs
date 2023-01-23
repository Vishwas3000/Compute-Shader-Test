using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ProceduralCircle))]
public class ProceduralCircle_Inspector : Editor
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        ProceduralCircle controller = (ProceduralCircle)target;

        if(GUILayout.Button("Flip Emit"))
        {
            controller.FlipEmit();
        }

    }
}
