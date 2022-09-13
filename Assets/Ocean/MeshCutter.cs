using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;



[ExecuteAlways]
public class MeshCutter : MonoBehaviour
{

    public Transform cuttingPlane;
    public string layerToBake;

    public MeshFilter cutMeshRenderer;

    public int processStepsPerFrame;


    // Start is called before the first frame update
    public void OnEnable()
    {
        Bake();
        
    }

    public bool meshSaved = false;
    public string meshSaveName;
    public void Bake(){

        meshSaved = false;
        
        int layerID = LayerMask.NameToLayer(layerToBake);
        GameObject[] cutGameObjects = FindGameObjectsWithLayer( layerID );

        Mesh cuttingMesh = new Mesh();

        cuttingMesh.indexFormat = IndexFormat.UInt32;
        
        List<Transform> meshesToCut = new List<Transform>();

        List<CombineInstance> combineList = new List<CombineInstance>();

        for( var i = 0; i < cutGameObjects.Length; i++ ){

            var mf = cutGameObjects[i].GetComponent<MeshRenderer>();



            if( mf != null ){

                if( mf.enabled ){

                    meshesToCut.Add(mf.transform);
                    
                    CombineInstance cb = new CombineInstance();
                    cb.mesh = mf.transform.GetComponent<MeshFilter>().sharedMesh;
                    cb.transform = cutGameObjects[i].transform.localToWorldMatrix;
                    combineList.Add( cb );
                }


            }


        }

        cuttingMesh.CombineMeshes(combineList.ToArray());

        print(combineList.Count);

        CutMesh.CreateMeshCutProcess( cuttingMesh , cuttingPlane );

        //cutMeshRenderer.mesh = finalCutMesh;

        // Render
        //cutMeshRenderer.transform.gameObject.SetActive(true);
    }


    public void Update(){
        if( CutMesh.processing ){
            print("processing : " + CutMesh.processState  + " | " + CutMesh.amountInProcessState );
            int processState = CutMesh.processState;
            for( var i = 0; i < processStepsPerFrame; i++ ){
                CutMesh.Process();

                // only one process step per frame plz;
                if( CutMesh.processState != processState ){
                    break;
                }
            }
        }else{
            if( meshSaved == false ){
                meshSaved = true;
                cutMeshRenderer.mesh = CutMesh.mesh;
                AssetDatabase.CreateAsset( CutMesh.mesh , "Assets/CutMeshes/" + meshSaveName + ".asset" );
                AssetDatabase.SaveAssets();

            }
        }

        
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
