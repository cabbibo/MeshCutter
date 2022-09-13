using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Mathematics;

using UnityEngine.Rendering;


public  class CutMesh{

    


    public static Mesh inputMesh;
    public static Mesh mesh;
    public static  Transform cutPlane;

    public static List<List<Vector3>> faces;
    
    public static  Transform[] cutPlanes;
    // Start is called before the first frame update


    public static int totalEdgeCount;
    public static int totalTriCount;
    public static  int totalNormCount;



    // Update is called once per frame

    static MaterialPropertyBlock mpb;
    public static Material edgeMat;
    public static Material triMat;

    public static  int currentPlane;
    public static Transform currentCutPlane;

    public static List<edge> newEdges;
    public static List<edge> newEdgeCopy;
    public static List<List<edge>> edgeGroupings;


    public static Mesh CutMeshWithPlane( Mesh inMesh , Transform plane ){

        inputMesh = inMesh;

        Reset();
        Cut( plane );
        Flatten();

        return mesh;

    }


    
    public static Mesh CutMeshWithPlanes( Mesh inMesh , Transform[] planes ){

        inputMesh = inMesh;

        Reset();
        for( int i =0; i < planes.Length; i++){
            Cut( planes[i] );
        }
        Flatten();

        return mesh;

    }



    /*public void OnEnable(){
       
       
        currentPlane = 0;
        newEdges = new List<edge>();
       
        Reset();
        Cut( cutPlane );
        Flatten();
    }*/


    public static void CreateMeshCutProcess( Mesh inMesh , Transform plane  ){
        
        inputMesh = inMesh;
        Reset();
        SetUpCut( plane );

    }


    public static int currentCutFace = 0;
    public int totalFacesToCut;
    public  static Transform cuttingPlane;

    public  static bool currentlyCutting;

     static List<Vector3> newPoints;

    public static Vector3 cutPosition;
    public static Vector3 cutUp;


    public static int processState; 

    public static int currentEdgeGroup;

    public static bool processing;

    public static Mesh finalMesh;
    public static bool finishedProcessing;
    public static void SetUpCut( Transform plane ){
       
        cutPlane = plane;
        currentlyCutting = true;
        currentCutFace = faces.Count-1;


        currentCutPlane = cutPlane;
        cutPosition = cutPlane.position;
        cutUp = cutPlane.up;

        currentCutPosition = cutPosition;
        currentCutUp = cutUp;

    

        // First off, we make a new list of points that we will populate
        // with the points that are going to make up the new face 
        // that will be added from the plane cut
        newPoints = new List<Vector3>();

        processState = 0;

        currentEdgeGroup = 0;

        processing = true;
        finishedProcessing = false;
        finalMesh = null;



    }



    public static float amountInProcessState;
    public static void Process(){


        if( processing ){
        
            if( processState == 0){
                AddCutPointsForFace( currentCutFace );
                currentCutFace --;
                //Debug.Log(currentCutFace);

                amountInProcessState = 1 - ((float)currentCutFace/(faces.Count));



                if( currentCutFace < 0 ){
                    processState = 1;
                }
            }

            /*if( processingState == 1 ){
                RemoveDuplicateFaces();
            }*/


            int fullCount = 0;

            if( processState == 1 ){
                SetUpEdgeGroups();
                amountInProcessState = 1;
                processState = 2;
                fullCount = newEdges.Count;
                
            }

        

            if( processState == 2 ){

                if( newEdges.Count > 0 ){
                    amountInProcessState = 1- ((float)newEdges.Count/(float)fullCount);
                    List<edge> eg = MakeEdgeGroup(newEdges);
                    edgeGroupings.Add(eg);
                    
                }else{
                    processState = 3;
                }
                
            }

            if( processState == 3 ){


                AddEdgeGroupFace(currentEdgeGroup);
                currentEdgeGroup ++;

                amountInProcessState = (float)currentEdgeGroup / (float)edgeGroupings.Count;

                if( currentEdgeGroup ==  edgeGroupings.Count ){
                    processState = 4;
                }

            }


            if( processState == 4 ){
                GetAllNewPoints();
                processState = 5;
                amountInProcessState = 1;
            }

            if( processState == 5 ){
                Flatten();
                processing = false;
                finishedProcessing = true;
                amountInProcessState = 1;
            }

        }

    }



 static int edgeCount;
 static int totalSame;

 static bool[] edgeComplete;



public static void RemoveDuplicateFaces(){

}
public static void GetAllNewPoints(){
     if( newPoints.Count != 0 ){

        int same= 0;
        for(int i = 0;  i< newPoints.Count; i++ ){
            for( int j = 0; j < newPoints.Count; j++ ){
        
                if( newPoints[i] == newPoints[j] && i != j){
                    same ++;
                }
            }
        }

        //print("SAME: " + same);



        float[] angles = new float[ newPoints.Count ];


    Vector4[] full = new Vector4[newPoints.Count];


    // getting the centroid to compare angles too
    Vector3 centroid = new Vector3();
    for(int i = 0;  i< newPoints.Count; i++ ){
        centroid += newPoints[i];
    }

    centroid /= newPoints.Count;

    Vector3 startingVec = newPoints[0] - centroid;
    Vector3 perp = Vector3.Cross( startingVec , cutUp );

    // looping through and assigning all our points with
    // an addition 'angle' for order usage
    full[0] = newPoints[0];
    for(int i = 0; i < newPoints.Count; i++ ){

        float a = GetAngleBetween( newPoints[i] - centroid , startingVec , perp.normalized );
        full[i] = fullVec( newPoints[i] ,  a);

    }

    // here we sort the array by the actual angle
    Array.Sort(full, Vector4Compare);    
    Array.Reverse(full);

    // And then we reassign the sorted points
    for(int i = 0; i< newPoints.Count; i++ ){
        newPoints[i] = new Vector3( full[i].x , full[i].y , full[i].z);
    }

    //faces.Add( newPoints );

}
}
    public static void AddEdgeGroupFace(int i){
              List<edge> eg = edgeGroupings[i];
            List<Vector3> facePoints = new List<Vector3>();

            float3 centroid = 0;
            for( var j= 0; j < eg.Count; j++){

                centroid += eg[j].p1;
            }

            centroid /= eg.Count;

            

            for( var j= 0; j < eg.Count; j++){
                

                //print( eg[j].p2 * 100);
                //print( eg[(j+1)%eg.Count].p1 * 100 );
                facePoints.Add( new Vector3(eg[j].p2.x,eg[j].p2.y,eg[j].p2.z));

               // print( math.length( centroid-eg[j].p1 ));

            }

        

            faces.Add( facePoints );
    }
    public static void SetUpEdgeGroups(){
        
        edgeCount = newEdges.Count;
        totalSame = 0;

        edgeGroupings = new List<List<edge>>();
        edgeComplete = new bool[edgeCount];

        
        newEdgeCopy = new List<edge>(newEdges);


    }

    public static void AddCutPointsForFace(int id){
        
            var face = faces[id];

            // we will recreate the face by adding
            // only the points that are not cut! 
            List<Vector3> newFace = new List<Vector3>();

            List<Vector3> newCutPoints = new List<Vector3>();

   

            for( int i = 0; i < face.Count; i++ ){
            
                // Get two neighboring points in the face to see if they 
                // intersect the plane
                var p1 = face[i];
                var p2 = face[(i+1)%face.Count];

                if( p1 != p2 ){
                    

                    // Check both to see if they above the cut plane
                    bool p1Above = aboveCutPlane( p1 , cutPosition , cutUp);
                    bool p2Above = aboveCutPlane( p2 , cutPosition , cutUp);



                    
                    // if both below than its fine to just add this edge!
                    if( p1Above == false  && p2Above == false ){
                        SafeAdd(newFace,p1 );
                    } 

                    // If both points are above, then we arent
                    // goint to save either point
                    if( p1Above == true && p2Above == true ){

                    }


                    // If the cut plane intersects our edge
                    // then we need to add a new point which 
                    // exists on the cut plane
                    if( p1Above == true  && p2Above == false ){
                        // add projected p1 to the face list
                        Vector3 newPos = projectPoint(p1 , p2, cutPosition , cutUp);

                         SafeAdd(newFace,newPos ); 


                         // adding a new point to our list of new points
                         // for later use in creating the generated face
                         //SafeAdd(newPoints,newPos );

                         newCutPoints.Add(newPos);



                    }

                    // If the cut plane intersects our edge
                    // then we need to add a new point which 
                    // exists on the cut plane
                    if( p1Above == false && p2Above  == true ){
                        SafeAdd(newFace,p1);
                        SafeAdd(newFace,projectPoint(p2,p1, cutPosition , cutUp));
                        newCutPoints.Add( projectPoint(p2,p1, cutPosition , cutUp));
                    }
                }
            }


            if( newFace.Count == 0 ){
                faces.RemoveAt(id);   
            }else{
                faces[id] = newFace;
            }

            if( newCutPoints.Count == 2 ){
               edge e = new edge();
               e.p1 = newCutPoints[0];
               e.p2 = newCutPoints[1];
                newEdges.Add(e);
            }else if( newCutPoints.Count > 2 ){
                //print("NO NO NO NO NO ");
            }

            //print( newCutPoints.Count );
    }





    /*


    Reseting all points to a triangular prism
    By erasing the faces, and then pumping in
    new faces that match the triangular prism

    This is because we need SOMETHING to cut out of

    */

    public static void Reset(){



        faces = new List<List<Vector3>>();

        int[] triangles = inputMesh.triangles;
        Vector3[] vertices = inputMesh.vertices;
        for( var i = 0; i < triangles.Length; i+=3 ){
            List<Vector3> l = new List<Vector3>();
            l.Add(vertices[triangles[i]] );
            l.Add(vertices[triangles[i+1]] );
            l.Add(vertices[triangles[i+2]] );
            faces.Add(l);
        }



        newEdges = new List<edge>();


    }



    // Makes sure that when we add a new point,
    // Its not a duplicate as that will cause problems down teh line
    
    public static void SafeAdd( List<Vector3> points , Vector3 p ){


        bool canAdd = true;

        for( int i = 0; i < points.Count; i++ ){
            if( points[i] == p ){
                canAdd = false;
               // print("DUPLICATE POINTS");
            }
        }

        if( canAdd ){
            points.Add(p);
        }

    }


    public struct edge{
        public float3 p1;
        public float3 p2;
    }


    public void AddNewEdge( Vector3 p1 , Vector3 p2 ){

        edge e = new edge();
        e.p1 = p1;
        e.p2 = p2;
        newEdges.Add(e);

    }

    static Vector3 currentCutPosition;
    static Vector3 currentCutUp;














    /*


        CUT



    */

    // This is where the MAGIC happens, ill try and comment line line
    static void Cut( Transform cutPlane){


        currentCutPlane = cutPlane;
        Vector3 position = cutPlane.position;
        Vector3 up = cutPlane.up;

        currentCutPosition = position;
        currentCutUp = up;

        // First off, we make a new list of points that we will populate
        // with the points that are going to make up the new face 
        // that will be added from the plane cut
        List<Vector3> newPoints = new List<Vector3>();

        // for every face we will need to see what we need to  cut
        for( int id = faces.Count-1; id >= 0; id-- ){

            var face = faces[id];

            // we will recreate the face by adding
            // only the points that are not cut! 
            List<Vector3> newFace = new List<Vector3>();

            List<Vector3> newCutPoints = new List<Vector3>();

            for( int i = 0; i < face.Count; i++ ){
            
                // Get two neighboring points in the face to see if they 
                // intersect the plane
                var p1 = face[i];
                var p2 = face[(i+1)%face.Count];

                if( p1 != p2 ){
                    

                    // Check both to see if they above the cut plane
                    bool p1Above = aboveCutPlane( p1 , position , up);
                    bool p2Above = aboveCutPlane( p2 , position , up);



                    
                    // if both below than its fine to just add this edge!
                    if( p1Above == false  && p2Above == false ){
                        SafeAdd(newFace,p1 );
                    } 

                    // If both points are above, then we arent
                    // goint to save either point
                    if( p1Above == true && p2Above == true ){

                    }


                    // If the cut plane intersects our edge
                    // then we need to add a new point which 
                    // exists on the cut plane
                    if( p1Above == true  && p2Above == false ){
                        // add projected p1 to the face list
                        Vector3 newPos = projectPoint(p1 , p2, position , up);

                         SafeAdd(newFace,newPos ); 


                         // adding a new point to our list of new points
                         // for later use in creating the generated face
                         //SafeAdd(newPoints,newPos );

                         newCutPoints.Add(newPos);



                    }

                    // If the cut plane intersects our edge
                    // then we need to add a new point which 
                    // exists on the cut plane
                    if( p1Above == false && p2Above  == true ){
                        SafeAdd(newFace,p1);
                        SafeAdd(newFace,projectPoint(p2,p1, position , up));
                        newCutPoints.Add( projectPoint(p2,p1, position , up));
                    }
                }
            }


            if( newFace.Count == 0 ){
                faces.Remove(faces[id]);   
            }else{
                faces[id] = newFace;
            }

            if( newCutPoints.Count == 2 ){
               edge e = new edge();
               e.p1 = newCutPoints[0];
               e.p2 = newCutPoints[1];
               newEdges.Add(e);
            }else if( newCutPoints.Count > 2 ){
                //print("NO NO NO NO NO ");
            }

            //print( newCutPoints.Count );

        }




        //        print("NEW EDGES" + newEdges.Count );
        int edgeCount = newEdges.Count;
        int totalSame = 0;
    

        edgeGroupings = new List<List<edge>>();
        var edgeComplete = new bool[edgeCount];
        /*

        for( var i = 0; i < newEdges.Count; i++ ){
            //print( newEdges[0].p1.y );
        }

        int numEqual = 0;
        for( var i = 0; i < newEdges.Count; i++ ){
            var e1 = newEdges[i];


            for( var j=0; j < newEdges.Count; j++ ){
                if( i != j ){

                    var e2 = newEdges[j];
                    if( i == 0 ){
                        if( (Vector3)e1.p2 == (Vector3)e2.p1){
                            //print( "edgeEqual: " + j );
                        }

                           if( (Vector3)e1.p2 == (Vector3)e2.p2){
                            //print( "edgeEqual: " + j );
                        }
                                
                    }
                }
            }
        }*/
        

        //print( newEdges.Count );

        newEdgeCopy = new List<edge>(newEdges);
        while( newEdges.Count > 0 ){
            List<edge> eg = MakeEdgeGroup(newEdges);
           edgeGroupings.Add(eg);
        }





        for( var i = 0; i < edgeGroupings.Count; i++ ){

            List<edge> eg = edgeGroupings[i];
            List<Vector3> facePoints = new List<Vector3>();

            float3 centroid = 0;
            for( var j= 0; j < eg.Count; j++){

                centroid += eg[j].p1;
            }

            centroid /= eg.Count;

            

            for( var j= 0; j < eg.Count; j++){
                

                //print( eg[j].p2 * 100);
                //print( eg[(j+1)%eg.Count].p1 * 100 );
                facePoints.Add( new Vector3(eg[j].p2.x,eg[j].p2.y,eg[j].p2.z));

               // print( math.length( centroid-eg[j].p1 ));

            }

        

            faces.Add( facePoints );
        }

       

        

//        print("TOTAL SAME" + totalSame);
        
        // If we have a new point created ( AKA teh plane cut our face)
        // we are going to need to reorganize all of the points
        // so that they they are right hand friendly and will 
        // make good triangle lists when needed.

        // if our plane didn't interect the crystal though we 
        // can ignore this section

       // print( newPoints.Count);

        if( newPoints.Count != 0 ){

            int same= 0;
            for(int i = 0;  i< newPoints.Count; i++ ){
                for( int j = 0; j < newPoints.Count; j++ ){
            
                    if( newPoints[i] == newPoints[j] && i != j){
                        same ++;
                    }
                }
            }

            //print("SAME: " + same);



            float[] angles = new float[ newPoints.Count ];


            Vector4[] full = new Vector4[newPoints.Count];


            // getting the centroid to compare angles too
            Vector3 centroid = new Vector3();
            for(int i = 0;  i< newPoints.Count; i++ ){
                centroid += newPoints[i];
            }

            centroid /= newPoints.Count;

            Vector3 startingVec = newPoints[0] - centroid;
            Vector3 perp = Vector3.Cross( startingVec , up );

            // looping through and assigning all our points with
            // an addition 'angle' for order usage
            full[0] = newPoints[0];
            for(int i = 0; i < newPoints.Count; i++ ){

                float a = GetAngleBetween( newPoints[i] - centroid , startingVec , perp.normalized );
                full[i] = fullVec( newPoints[i] ,  a);

            }

            // here we sort the array by the actual angle
            Array.Sort(full, Vector4Compare);    
            Array.Reverse(full);

            // And then we reassign the sorted points
            for(int i = 0; i< newPoints.Count; i++ ){
                newPoints[i] = new Vector3( full[i].x , full[i].y , full[i].z);
            }

            //faces.Add( newPoints );
        
        }

    }


/*void OnDrawGizmosSelected()
    {


for( var i = 0; i< newEdgeCopy.Count; i++ ){
    
            Gizmos.color = Color.red;
      Gizmos.DrawLine(
                    this.transform.TransformPoint(newEdgeCopy[i].p1), 
                    this.transform.TransformPoint(newEdgeCopy[i].p2)
                );
}


        for( var i = 0; i < edgeGroupings.Count; i++ ){
            List<edge> eg = edgeGroupings[i];
            List<Vector3> facePoints = new List<Vector3>();

            

            float3 centroid = 0;
            for( var j= 0; j < eg.Count; j++){

                centroid += eg[j].p1;

                

            }

            centroid /= eg.Count;

            
            Gizmos.color = Color.white;
            for( var j= 0; j < eg.Count; j++){



               Gizmos.DrawLine(
                    this.transform.TransformPoint(eg[j].p1), 
                    this.transform.TransformPoint(eg[j].p2)
                );

                   Gizmos.DrawLine(
                    this.transform.TransformPoint(eg[j].p1), 
                    this.transform.TransformPoint(centroid)
                );

                   Gizmos.DrawLine(
                    this.transform.TransformPoint(eg[j].p2), 
                    this.transform.TransformPoint(centroid)
                );

            }
        }
  
    }*/

private static  int Vector4Compare(Vector4 value1, Vector4 value2)
     {
          if (value1.w < value2.w)
         {
             return -1;
         }
         else if(value1.w == value2.w)
         {
            return 0;
         }else{
             return 1;
         }
     }


    static float GetAngleBetween( Vector3 d1 , Vector3 d2 , Vector3 perp){

        float a = Vector3.Angle( d1 , d2 );

        float m = Vector3.Dot( d1 , perp );

        if( m > 0 ){
            a =  360-a;
        }

        return a;

    }

    static bool aboveCutPlane(  Vector3 position ,  Vector3 planePosition , Vector3 planeUp  ){
        Vector3 d = position - planePosition;
        float dist = Vector3.Dot( planeUp , d );
        return (dist > 0);
    }



    // project point onto plane
    static Vector3 projectPoint( Vector3 p1 , Vector3 p2 , Vector3 planePosition ,Vector3 planeUp  ){

        Vector3 n = planeUp;
        Vector3 u = (p2-p1).normalized;
        Vector3 w = p2 - planePosition;

        float baseDot = Vector3.Dot(n,u);
        float topDot = -Vector3.Dot( n,w);

        float d =  topDot / baseDot;

        Vector3 projectedPoint = p2 + u * d * 1;
        return projectedPoint;
       
    }


    // This is where we create the mesh!
    // we just ad informations for all of our points
    // we have created and generate the list of 
    // indicies that reference those points
    public static void Flatten(){

        List<Vector3> allEdgePoints = new List<Vector3>();
        List<Vector3> allTriPoints = new List<Vector3>();
        List<Vector3> allTriNorms = new List<Vector3>();
        List<Vector2> allTriUVs = new List<Vector2>();
        List<Color> allTriColors = new List<Color>();

        List<int> allTriIDs = new List<int>();

        int faceID = 0;
        int index = 0;
        foreach( var face in faces){

             

            if( face.Count == 3 ){

                AddTri( face[0],face[1],face[2],Color.green, ref index, allTriPoints, allTriIDs, allTriNorms,allTriColors);

             

            }else if( face.Count==4 ){
                AddTri( face[0],face[1],face[2], Color.red, ref index, allTriPoints, allTriIDs, allTriNorms,allTriColors);
                 AddTri( face[0],face[2],face[3],Color.red,  ref index, allTriPoints, allTriIDs, allTriNorms,allTriColors);
            }else{


                var t  = new Triangulator( face );
                int[] indices = t.Triangulate();
                //print("longtriii");
                //print(indices.Length);
                for( var i = 0; i < indices.Length; i+=3 ){
                      AddTri( face[indices[i+0]],face[indices[i+1]],face[indices[i+2]],Color.blue,  ref index, allTriPoints, allTriIDs, allTriNorms,allTriColors);
          
                }

                /*
                print("Long tri");
                List<Vector3> faceCopy = new List<Vector3>(face);
                List<Vector2> planePoints = new List<Vector2>();

                for( var i = 0; i < face.Count; i++ ){

                    Vector3 p3D = currentCutPlane.InverseTransformPoint( face[i] );
                    Vector2 p = new Vector2(p3D.x,p3D.y);
                    planePoints.push( p )
                }
                List<Vector3> triList = new List<Vector3>(face);

                List<Vector4> angles = new List<Vector4>();



                var totalConvex = 0;
                var totalConcave = 0;


                int faceTmpID = 0;
                

*/

                /*for( var i = 0; i < faceCopy.Count; i++ ){
                    int id1 = i % faceCopy.Count;
                    int id2 = (i+1) % faceCopy.Count;
                    int id3 = (i+2) % faceCopy.Count;

                    Vector3 cross = Vector3.Cross( faceCopy[id1] - faceCopy[id2] , faceCopy[id1] - faceCopy[id3] ).normalized;
                    float match = Vector3.Dot( cross , currentCutUp );

                    angles.Add(new Vector4(id1,id2,id3,match));

                }*/
                
/*
                var iterationCount = 0;

                while( faceCopy.Count > 4){

                    iterationCount ++;
                    if( iterationCount > 1000 ){
                        print("breaking");
                        break;
                    }

                    int id1 = faceTmpID % faceCopy.Count;
                    int id2 = (faceTmpID+1) % faceCopy.Count;
                    int id3 = (faceTmpID+2) % faceCopy.Count;


                    Vector3 p1 = faceCopy[id1];
                    Vector3 p2 = faceCopy[id2];
                    Vector3 p3 = faceCopy[id3];

                    new List<Vector3>() tmpCopy = new List<Vector3>( faceCopy );

                    tmpCopy.Remove( earToCheck );

                    Vector3 centroid = (p1 + p2 + p3) / 3;

                    Vector3 randomRay









                    for( var i =0; i < )

                  


                    Vector3 cross = Vector3.Cross( (faceCopy[id1] - faceCopy[id2]).normalized , (faceCopy[id1] - faceCopy[id3]).normalized ).normalized;
                    float match = Vector3.Dot( cross , currentCutUp );

                    

                    if( match > 0 ){
                        totalConcave += 1;
                        Color c = Color.HSVToRGB( (float)totalConcave/(float)face.Count , .5f, 1 );
                        AddTri( faceCopy[id1],  faceCopy[id2],faceCopy[id3],c, ref index, allTriPoints, allTriIDs, allTriNorms,allTriColors);
                        faceCopy.Remove( faceCopy[id2]);
                        
                        // Restart Checking!
                        faceTmpID = 0;
                    }else{
                        faceTmpID += 1;
                    }

                } */
/*
print("Convex :"+ totalConvex);
print("Concave :"+ totalConcave);



                Vector3 centroid = new Vector3();

                

                for(int i = 0; i < face.Count; i++ ){

                    centroid += face[i];
                    allEdgePoints.Add(face[i]);
                    allEdgePoints.Add(face[(i+1)%face.Count]);

                }

                centroid /= face.Count;


                for( int i = 0; i < face.Count; i++ ){

                    int id1 = i;
                    int id2 = (i+1)%face.Count;

                    allTriPoints.Add( centroid );
                    allTriPoints.Add( face[id1]);
                    allTriPoints.Add( face[id2]);

                    allTriIDs.Add(index++);
                    allTriIDs.Add(index++);
                    allTriIDs.Add(index++);

                    allTriNorms.Add( Vector3.Cross( (face[id1] - centroid).normalized , (face[id2] - centroid).normalized).normalized );
                    allTriNorms.Add( Vector3.Cross( (face[id1] - centroid).normalized , (face[id2] - centroid).normalized).normalized );
                    allTriNorms.Add( Vector3.Cross( (face[id1] - centroid).normalized , (face[id2] - centroid).normalized).normalized );


                }

*/


            }


            

            faceID ++;

        }





        totalEdgeCount = allEdgePoints.Count;
        totalTriCount = allTriPoints.Count;
        totalNormCount = allTriNorms.Count;

        mesh = new Mesh();
        
        mesh.indexFormat = IndexFormat.UInt32;
        mesh.vertices = allTriPoints.ToArray();
        mesh.colors = allTriColors.ToArray();
        mesh.normals = allTriNorms.ToArray();
        mesh.triangles = allTriIDs.ToArray();

//        Debug.Log("MESH VERT COUNT: " + mesh.vertices.Length );
        //GetComponent<MeshFilter>().mesh = mesh;

    }

    static Vector4 fullVec( Vector3 v , float id ){

        return new Vector4( v.x , v.y , v.z , id);
    }

    static void AddTri( Vector3 p1 ,  Vector3 p2 ,  Vector3 p3, Color color, ref int index , List<Vector3> points, List<int> ids, List<Vector3> norms  , List<Color> colors  ){
        points.Add( p1 );
        points.Add( p2);
        points.Add( p3);

        ids.Add(index++);
        ids.Add(index++);
        ids.Add(index++);

        Vector3 norm =  Vector3.Cross( (p2 - p1).normalized , (p3 - p1).normalized).normalized;

        norms.Add( norm );
        norms.Add( norm );
        norms.Add( norm );

        colors.Add(color);
        colors.Add(color);
        colors.Add(color);

    }







        // Finds a list of edges that make up a
        // connected polygon
        static List<edge> MakeEdgeGroup(List<edge> newEdges){

            List<edge> edgeGroup = new List<edge>();

            edge startEdge = newEdges[0];

            newEdges.Remove(startEdge);

            //print( newEdges.Count );

            CheckForNextEdge( newEdges, startEdge , startEdge, edgeGroup);

            //print(edgeGroup.Count);
            return edgeGroup;
        }

        //Checks to figure out which edge is next, and continues going around til we 
        // have reached our original edge!
        static  void CheckForNextEdge(List<edge> newEdges, edge startEdge , edge currEdge , List<edge> edgeGroup ){

            bool edgeFound = false;
            edge edgeToAdd = new edge();

            float minSize = 1000;

            for( int i = 0; i < newEdges.Count; i++){

                edge e2 = newEdges[i];

                if(  currEdge.p2.Equals( e2.p1 ) ){

                    //print("EdgeFoundAt : "+ i);
                    edgeFound = true;
                    edgeToAdd = e2;
                    minSize=0;
                    
                    break;
                }else if( currEdge.p2.Equals( e2.p2 )){
                    edgeFound = true;
                    edgeToAdd = e2;

                   // print("EdgeFoundAt : "+ i);
                    // Flip edges so in correct order!

                    edge newEdge = new edge();
                    newEdge.p1 = e2.p2;
                    newEdge.p2 = e2.p1;
                    edgeToAdd = newEdge;
                  

                    newEdges[i] = newEdge;
                    minSize=0;

                    break;
                }else{
                    float3 dif = currEdge.p2 - e2.p2;

                    float l = math.length(dif);

                    if( l < minSize){
                        minSize = l;
                    }

                    //print("DIST: " + l);
                    
                    //print( "Dist: " + Mathf.Min((currEdge.p2 - e2.p2).magnitude,( currEdge.p2 - e2.p1 ).magnitude));
                }


               /* if( edgeFound ){

                    //print("EdgeFoundAt : "+ i);
              
                    
                    edgeToAdd = e2;

                }*/

            }

            if( edgeFound){

                edgeGroup.Add(edgeToAdd);
                newEdges.Remove(edgeToAdd);
               // print("edgeAdded");

                if( !edgeToAdd.p2.Equals( startEdge.p1 ) ){
                    CheckForNextEdge( newEdges, startEdge , edgeToAdd, edgeGroup);
                }else{
                    //print("EdgeGroupComplete");
                }

            }else{
                
                //print(minSize);

                //print("No edges To Add Found! ");
            }

        }

public static List<Vector3> cutPositions;
public static List<Vector3> cutDirections;







/*

    Here is where the shape of the gem 
    is actually made! we set up a list of 
    'cuts' that we are goint to make. 

    If you want to play with how the crystal looks
    this is where to do it!

*/
   /* public void SetUpGemCut(){

        cutPositions = new List<Vector3>();
        cutDirections = new List<Vector3>();
        Vector3 topPoint = new Vector3( 0, crystalHeight, 0);
        Vector3 d; Vector3 p;

        
        // 3 more cuts to turn the triangular prism
        // into a hexagon. ( the randomness factor will make some sides bigger than others)
        for( int i = 0; i < 3; i++  ){
            float a = (((float)i)/(float)3) * 2 * Mathf.PI;

            float x = Mathf.Sin(a);
            float y = -Mathf.Cos(a);
             p  = new Vector3(x , 0 , y) * crystalRadius * .5f * UnityEngine.Random.Range(.5f , 1.5f );
             d = new Vector3(x , 0 , y );

            cutPositions.Add(p);
            cutDirections.Add(d);
        }

       


 
        // doing the top 'cuts' of the crystal
        for(int i = 0;  i< 6; i++){

            float a = (((float)i )/(float)6) * 2 * Mathf.PI;
            
            float r = crystalRadius * .5f;
            float x =  Mathf.Sin(a) * r;
            float y = -Mathf.Cos(a) * r;
            

            Vector3 dir = new Vector3( x , crystalRadius *  cutAngle , y ).normalized;

            p  = new Vector3(x , crystalHeight , y) ;
        
            // move the cut position off by the normal to create some diversity in cut
            p -= dir * UnityEngine.Random.Range( -crystalRadius * .3f, crystalRadius * .5f);


            d = topPoint - p;

            Vector3 tang = Vector3.Cross(dir, Vector3.up);
            d = Vector3.Cross(d , tang );
            d = d.normalized;


            cutPositions.Add(p);
            cutDirections.Add(dir.normalized);
        }



    }*/
}
