using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;


[ExecuteAlways]
public class Ocean : MonoBehaviour
{

    public string oceanColliderLayerName;
    public List<Transform> oceanColliders;

    public MeshFilter debugOceanCut;

    public float oceanSize;
    public int oceanResolution;

    public float nearClip;
    public float oceanMaxDepth;

    public OrthographicDepthRenderer depthRenderer;

    public int oceanMeshResolution;
    public float oceanMeshSize;

    public bool RunInEditMode;

    public RippleRenderer rippleRenderer;



    public void Bake(){
        
        int layerID = LayerMask.NameToLayer(oceanColliderLayerName);
        GameObject[] oceanGameObjects = FindGameObjectsWithLayer( layerID );

        Mesh cuttingMesh = new Mesh();

        cuttingMesh.indexFormat = IndexFormat.UInt32;
        
        oceanColliders = new List<Transform>();

        List<CombineInstance> combineList = new List<CombineInstance>();

        for( var i = 0; i < oceanGameObjects.Length; i++ ){

            var mf = oceanGameObjects[i].GetComponent<MeshRenderer>();



            if( mf != null ){

                if( mf.enabled ){

                    oceanColliders.Add(mf.transform);
                    
                    CombineInstance cb = new CombineInstance();
                    cb.mesh = mf.transform.GetComponent<MeshFilter>().sharedMesh;
                    cb.transform = oceanGameObjects[i].transform.localToWorldMatrix;
                    combineList.Add( cb );
                }


            }


        }

        cuttingMesh.CombineMeshes(combineList.ToArray());

        Mesh finalCutMesh = CutMesh.CutMeshWithPlane( cuttingMesh , transform );

        debugOceanCut.mesh = finalCutMesh;



        // Render
        debugOceanCut.transform.gameObject.SetActive(true);

        depthRenderer.camSize = oceanSize;
        depthRenderer.renderSize = oceanResolution;
        depthRenderer.cam.nearClipPlane = nearClip;
        depthRenderer.cam.farClipPlane = oceanMaxDepth;
        depthRenderer.transform.localPosition = new Vector3(0,nearClip * 1.001f ,0);

        debugOceanCut.transform.position = Vector3.zero;
        debugOceanCut.transform.rotation = Quaternion.identity;
        depthRenderer.Set();

        debugOceanCut.transform.gameObject.SetActive(false);


        rippleRenderer.InitRenderTextures();
        rippleRenderer.depthTexture = depthRenderer.texture;


    


    }

    void Update(){
        GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_DepthTexture",depthRenderer.texture);
        GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Size",oceanMeshSize);
    }


    // Start is called before the first frame update
    void OnEnable()
    {
       Bake();

       MakeOceanMesh();
        
     #if UNITY_EDITOR 
        EditorApplication.update += Always;
    #endif
    }


void Always(){    
  #if UNITY_EDITOR 
  if( RunInEditMode ){ EditorApplication.QueuePlayerLoopUpdate();}
  #endif
}


    public void MakeOceanMesh(){

        Vector3[] positions = new Vector3[ oceanMeshResolution * oceanMeshResolution ];
        Vector2[] uvs = new Vector2[ oceanMeshResolution * oceanMeshResolution ];
        int[] tris = new int[ (oceanMeshResolution-1) *(oceanMeshResolution-1) * 3 * 2 ];


        Mesh mesh = new Mesh();

        int index = 0;
    

        for( var i = 0; i< oceanMeshResolution; i++ ){
            for( var j = 0; j < oceanMeshResolution; j++ ){

                float x = (float)i / (float)oceanMeshResolution;
                float y = (float)j / (float)oceanMeshResolution;

                positions[i * oceanMeshResolution + j] = new Vector3(x-.5f,0,y-.5f) * oceanMeshSize;
                uvs[i * oceanMeshResolution + j] = new Vector2(x,y);


                // do the IDS
                if( j < oceanMeshResolution-1 && i < oceanMeshResolution-1){

                    tris[index++] =     i * oceanMeshResolution + j;
                    tris[index++] =     i * oceanMeshResolution + j+1;
                    tris[index++] = (i+1) * oceanMeshResolution + j+1;
                    tris[index++] =     i * oceanMeshResolution + j;
                    tris[index++] = (i+1) * oceanMeshResolution + j+1;
                    tris[index++] = (i+1) * oceanMeshResolution + j;

                }

            }
        }

        mesh.vertices = positions;
        mesh.uv = uvs;
        mesh.triangles = tris;

        GetComponent<MeshFilter>().mesh = mesh;


    }


    GameObject[] FindGameObjectsWithLayer ( int layer) {
        var goArray = UnityEngine.Object.FindObjectsOfType<GameObject>() ;
        var goList = new List<GameObject>();
        for (var i = 0; i < goArray.Length; i++) {
            if (goArray[i].layer == layer) {
                goList.Add(goArray[i]);
            }
        }
        if (goList.Count == 0) {
            return null;
        }
        return goList.ToArray();
    }


   


}
