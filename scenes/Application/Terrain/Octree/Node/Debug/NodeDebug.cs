using Godot;
using System;
using System.Runtime.CompilerServices;

namespace Octree
{
    public partial class Node
    {
        public static bool debugVisualsEnabled = false;

        static Node3D meshNode; // zbavit se?


        static Shader debugMaterialShader;
        ShaderMaterial debugMaterial;

        Color debugColor;
        MeshInstance3D debugBox;


        static void InitDebugStatic()
        {
            debugMaterialShader = GD.Load<Shader>("res://scenes/Application/Terrain/Octree/Node/Debug/NodeDebugShader.gdshader");
        }

        void InitDebug()
        {
            this.debugColor = new Color(1.0f, 1.0f, 1.0f);
            this.debugBox = null;

            this.debugMaterial = new ShaderMaterial();
            debugMaterial.Shader = debugMaterialShader;
            debugMaterial.SetShaderParameter("color", this.debugColor);
        }

        void CreateDebugBox()
        {
            if (!debugVisualsEnabled) return;
            if (this.debugBox != null) return;
            // set box
            BoxMesh boxMesh = new BoxMesh();
            boxMesh.Size = new Vector3(this.size, this.size, this.size);
            this.debugBox = new MeshInstance3D();
            this.debugBox.Mesh = boxMesh;
            this.debugBox.Position = this.position + new Vector3(this.size, this.size, this.size) * 0.5f;
            // set shader
            this.debugBox.SetSurfaceOverrideMaterial(0, debugMaterial);
            // add mesh
            meshNode.AddChild(this.debugBox);
        }

        void RemoveDebugBox()
        {
            if (!debugVisualsEnabled) return;
            if (this.debugBox == null) return;

            meshNode.RemoveChild(this.debugBox);
            this.debugBox.QueueFree();
            this.debugBox = null;
        }

        public static void EnableDebugVisuals(bool enabled)
        {
            debugVisualsEnabled = enabled;
        }

        public static void SetMeshParentNode(Node3D meshParentNode)
        {
            meshNode = meshParentNode;
        }

        public void SetDebugColor(Color color)
        {
            this.debugColor = color;
            debugMaterial.SetShaderParameter("color", this.debugColor);
        }
    }
}