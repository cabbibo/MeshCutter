using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.Mathematics;


[ExecuteAlways]
public class Wave : MonoBehaviour
{

    public bool RunInEditMode;
    public int numPointsInString;

    public float velMultiplier;


    public Texture heightMap;

    public float3[] pos;
    public float3[] vel; 
    public float3[] force; 


    // Update is called once per frame
    void Update()
    {
        //print("hmmm");
        for( var i =0; i < pos.Length; i++ ){
            force[i] = 0;
        
        }

        for( var i = 0;  i < pos.Length; i++){
            vel[i] += force[i];
            pos[i] += vel[i] * velMultiplier;
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;

        for( var i = 0; i< pos.Length; i++){
            Gizmos.DrawWireCube(pos[i], .01f * Vector3.one);
        }

    }


        // Start is called before the first frame update
    void OnEnable()
    {

       // print("why no worky");
 
        pos = new float3[ numPointsInString ];
        vel = new float3[ numPointsInString ];
        force = new float3[ numPointsInString ];

        for( var i = 0; i < pos.Length; i++ ){
            vel[i] = new float3(0,0,1);
            pos[i] = Vector3.right * ((float)i/(float)pos.Length) ;
        }

        #if UNITY_EDITOR 
            EditorApplication.update += Always;
        #endif
    }

    void OnDisable(){
        #if UNITY_EDITOR 
            EditorApplication.update -= Always;
        #endif
    }

    void Always(){    
        #if UNITY_EDITOR 
            if( RunInEditMode ){ EditorApplication.QueuePlayerLoopUpdate();}
        #endif
    }

}
