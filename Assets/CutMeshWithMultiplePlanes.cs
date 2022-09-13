using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteAlways]
public class CutMeshWithMultiplePlanes : MonoBehaviour
{
  

  public Transform[] transforms;
  void OnEnable(){

      Mesh m = GetComponent<MeshFilter>().sharedMesh;

     GetComponent<MeshFilter>().mesh =  CutMesh.CutMeshWithPlane(m,transforms[0]);


  }
}
