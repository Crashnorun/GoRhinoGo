using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
//using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Diagnostics;
using Rhino;
using Rhino.Runtime.InProcess;
using Rhino.Geometry;
using Rhino.PlugIns;

using GH_IO.Serialization;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Types;


public class RhinoInside : MonoBehaviour {

    public Rhino.Runtime.InProcess.RhinoCore rhinoCore;
    //public string pathGH = @"C:\Users\Georgios\Desktop\CoolMeshBoolean.gh";
    public List<GeometryBase> output = new List<GeometryBase>();

    private Rhino.RhinoDoc doc = null;

    // Use this for initialization
    void Start () {
        
        ResolveEventHandler OnRhinoCommonResolve = null;
        AppDomain.CurrentDomain.AssemblyResolve += OnRhinoCommonResolve = (sender, args) =>
        {
            const string rhinoCommonAssemblyName = "RhinoCommon";
             var assemblyName = new AssemblyName(args.Name).Name;

            if (assemblyName != rhinoCommonAssemblyName)
                return null;

            AppDomain.CurrentDomain.AssemblyResolve -= OnRhinoCommonResolve;

            string rhinoSystemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino WIP", "System");
            return Assembly.LoadFrom(Path.Combine(rhinoSystemDir, rhinoCommonAssemblyName + ".dll"));
        };

        //rhinoCore = new RhinoCore(new string[] { $"/scheme=UNITY", "/nosplash", "/runscript=\"_Grasshopper\"" }, WindowStyle.Normal);
        rhinoCore = new RhinoCore(new string[] { $"/scheme=UNITY", "/nosplash" }, WindowStyle.Normal);
        doc = Rhino.RhinoDoc.ActiveDoc;
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp("e"))
        {
            Debug.Log("Sending to Rhino");
            UnityToRhino();
        }
        
	}

    /// <summary>
    /// Convert a list of unity game objects to Rhino meshes
    /// </summary>
    /// <param name="objs"></param>
    /// <returns></returns>
    void UnityToRhino()
    {

        // list of rhino meshes to return
        List<Rhino.Geometry.Mesh> returnObjs = new List<Rhino.Geometry.Mesh>();

        // extract each unity mesh
        foreach (MeshFilter meshFilter in FindObjectsOfType(typeof(MeshFilter)))
        {
            Debug.Log("Fetching mesh filter");
            // get mesh from game object
            UnityEngine.Mesh mesh = meshFilter.mesh;
            // Vector3[] vertices = mesh.vertices;
            // Vector3[] normals = mesh.normals;

            Rhino.Geometry.Mesh RHmesh = new Rhino.Geometry.Mesh();

            UnityEngine.Transform loc = meshFilter.transform;

            // extract each face
            foreach (Vector3 vect in mesh.vertices)
            {
                Vector3 pos = Vector3.Scale(vect,loc.localScale);
                RHmesh.Vertices.Add(pos.x + loc.localPosition.x, pos.z + loc.localPosition.z, pos.y + loc.localPosition.y);
            }

            // extract mesh faces
            for (int i = 0; i < mesh.triangles.Length; i += 3)
            {
                RHmesh.Faces.AddFace(mesh.triangles[i], mesh.triangles[i + 1], mesh.triangles[i + 2]);
            }
            RHmesh.Normals.ComputeNormals();
            
            doc.Objects.AddMesh(RHmesh);
        }
        doc.Views.Redraw();
    }

    void evaluateGHdef()
    {
        
        Debug.Log("1");

        string filePath = string.Empty;
        
       

       using (OpenFileDialog openFileDialog = new OpenFileDialog())
       {
         openFileDialog.Filter = "Grasshopper Binary (*.gh)|*.gh|Grasshopper Xml (*.ghx)|*.ghx";
         openFileDialog.FilterIndex = 1;
         openFileDialog.RestoreDirectory = true;

         switch(openFileDialog.ShowDialog())
         {
           case DialogResult.OK:     filePath = openFileDialog.FileName;
                    Debug.Log(filePath);
                    break;
                    
         }
       }
       
        var archive = new GH_Archive();
        archive.ReadFromFile(filePath);

        
        var definition = new GH_Document();
        archive.ExtractObject(definition, "Definition");
        

    Debug.Log("4");
     var outputs = new List<KeyValuePair<string, List<GeometryBase>>>();
     foreach (var obj in definition.Objects)
     {
         var param = obj as IGH_Param;
         Debug.Log("5");
         if (param == null)
             continue;
         Debug.Log("6");
         if (param.Sources.Count == 0 || param.Recipients.Count != 0)
             continue;
         Debug.Log("7");
         try
         {
             param.CollectData();
             param.ComputeData();
         }
         catch (Exception e)
         {
             //Debug.Fail(e.Source, e.Message);
             Debug.Log("8");
             param.Phase = GH_SolutionPhase.Failed;
             //result = Result.Failed;
         }

         Debug.Log("9");
         var volatileData = param.VolatileData;
         for (int p = 0; p < volatileData.PathCount; ++p)
         {
             foreach (var goo in volatileData.get_Branch(p))
             {
                 switch (goo.GetType().ToString())
                 {
                     case "GH_Point":
                         GH_Point point = (GH_Point) goo;
                             output.Add(new Rhino.Geometry.Point(point.Value));
                     break;

                     case "GH_Curve":
                         GH_Curve curve = (GH_Curve)goo;
                             output.Add(curve.Value);
                    break;

                     case "GH_Brep":
                         GH_Brep brep = (GH_Brep)goo;
                         output.Add(brep.Value);
                         break;

                     case "GH_Mesh":
                         GH_Mesh mesh = (GH_Mesh)goo;
                         output.Add(mesh.Value);
                         break;


                 }
             }
         }
         Debug.Log("10");
         if (output.Count > 0)
         {
             outputs.Add(new KeyValuePair<string, List<GeometryBase>>(param.Name, output));
             Debug.Log("11");
         }
     }
        Debug.Log("There are " + output.Count() + "GH objects in your list");

        //return outputs;

    }



}

    
