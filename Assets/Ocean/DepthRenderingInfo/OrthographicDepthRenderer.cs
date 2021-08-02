using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class OrthographicDepthRenderer : MonoBehaviour
{

  public int renderSize;
  public float camSize;
  public RenderTexture texture;
  
  public string layerToRender;


  public Renderer debugRenderer;
  public Camera cam;

  public Shader replacementShader;



  private RenderTextureDescriptor textureDescriptor;

  public void OnEnable(){

    //Set();
  }

  public void Update(){
    //Set();
  }



  public void Set(){
   
    textureDescriptor = new RenderTextureDescriptor( renderSize,renderSize,RenderTextureFormat.Depth,24);
    texture = RenderTexture.GetTemporary( textureDescriptor );
    //texture.filterMode= FilterMode.Point;
 
    cam.targetTexture = texture;
    cam.orthographicSize = camSize; 
    cam.depthTextureMode = DepthTextureMode.DepthNormals;
    ///print( texture.depth);
    if( replacementShader != null ){
    cam.SetReplacementShader(replacementShader,null);
    }
    cam.SetTargetBuffers( texture.colorBuffer , texture.depthBuffer );
    cam.Render();


    if( debugRenderer ){
        debugRenderer.sharedMaterial.SetTexture("_MainTex", texture );
        debugRenderer.transform.localScale = Vector3.one * cam.orthographicSize * 2;
    }

 

  }


}

