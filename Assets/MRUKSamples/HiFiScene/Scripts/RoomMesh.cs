// Copyright (c) Meta Platforms, Inc. and affiliates.

using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;
using UnityEngine;

namespace Meta.XR.MRUtilityKitSamples.HiFiScene
{
    [MetaCodeSample("MRUKSample-HiFiScene")]
    public class RoomMesh : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            MRUK.Instance.SceneLoadedEvent.AddListener(SceneLoadedEvent);
        }

        /// <summary>
        /// Creates a Unity Mesh from the RoomMeshData property.
        /// Each face in the room mesh will be created as a separate submesh.
        /// This property will be null if EnableHighFidelityScene is false.
        /// </summary>
        /// <returns>A Unity Mesh created from the room mesh data, or null if RoomMeshData is not available.</returns>
        private Mesh CreateMeshFromRoomMeshData(MRUKRoom.RoomMesh roomMesh)
        {
            Mesh mesh = new Mesh();

            // Set vertices
            mesh.vertices = roomMesh.Vertices.ToArray();

            // Set submesh count
            mesh.subMeshCount = roomMesh.Faces.Count;

            // Create submeshes for each face
            for (int i = 0; i < roomMesh.Faces.Count; i++)
            {
                // Set triangles for this submesh
                mesh.SetTriangles(roomMesh.Faces[i].Indices.ToArray(), i);
            }

            return mesh;
        }

        private void SceneLoadedEvent()
        {
            var room = MRUK.Instance.GetCurrentRoom();
            if (room != null && room.RoomMeshData != null)
            {
                transform.position = room.transform.position;
                transform.rotation = room.transform.rotation;
                var mesh = CreateMeshFromRoomMeshData(room.RoomMeshData.Value);
                if (room.RoomMeshData != null && mesh != null)
                {
                    var roomMesh = room.RoomMeshData.Value;
                    // Add MeshFilter if it doesn't exist
                    MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
                    if (meshFilter == null)
                    {
                        meshFilter = gameObject.AddComponent<MeshFilter>();
                    }

                    // Add MeshRenderer if it doesn't exist
                    MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
                    if (meshRenderer == null)
                    {
                        meshRenderer = gameObject.AddComponent<MeshRenderer>();
                    }

                    // Create materials for each submesh
                    int submeshCount = mesh.subMeshCount;
                    Material[] materials = new Material[submeshCount];

                    var shader = Shader.Find("Universal Render Pipeline/Lit");

                    // Create a material for each submesh with different colors to distinguish them
                    for (int i = 0; i < submeshCount; i++)
                    {
                        // Create a local color variable
                        Color color = new Color(0.5f, 0.5f, 0.5f, 1.0f); // Default gray color

                        // Assign different colors to different semantic types if available
                        if (i < roomMesh.Faces.Count)
                        {
                            var semanticLabel = roomMesh.Faces[i].SemanticLabel;

                            // Assign colors based on semantic label
                            switch (semanticLabel)
                            {
                                case MRUKAnchor.SceneLabels.FLOOR:
                                    color = new Color(0.2f, 0.6f, 0.2f, 1.0f); // Green for floor
                                    break;
                                case MRUKAnchor.SceneLabels.CEILING:
                                    color = new Color(0.8f, 0.8f, 0.8f, 1.0f); // White for ceiling
                                    break;
                                case MRUKAnchor.SceneLabels.WALL_FACE:
                                    color = new Color(0.6f, 0.6f, 0.8f, 1.0f); // Blue for walls
                                    break;
                                case MRUKAnchor.SceneLabels.INVISIBLE_WALL_FACE:
                                    color = new Color(0.8f, 0.3f, 0.8f, 1.0f); // Purple for invisible walls
                                    break;
                                case MRUKAnchor.SceneLabels.INNER_WALL_FACE:
                                    color = new Color(0.4f, 0.4f, 0.6f, 1.0f); // Darker blue for inner walls
                                    break;
                                case MRUKAnchor.SceneLabels.WINDOW_FRAME:
                                    color = new Color(0.7f, 0.9f, 1.0f, 1.0f); // Light blue for windows
                                    break;
                                case MRUKAnchor.SceneLabels.DOOR_FRAME:
                                    color = new Color(0.6f, 0.4f, 0.2f, 1.0f); // Brown for doors
                                    break;
                            }
                        }

                        // Create the material
                        materials[i] = new Material(shader)
                        {
                            color = color
                        };
                    }

                    // Assign the materials to the renderer
                    meshRenderer.materials = materials;

                    // Assign the mesh to the MeshFilter
                    meshFilter.mesh = mesh;
                }
            }
        }
    }
}
