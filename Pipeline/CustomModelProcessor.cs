using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.ComponentModel;

namespace Pipeline
{
    /// <summary>
    /// Processor allows custom effects to be assigned to a Model.
    /// </summary>
    [ContentProcessor(DisplayName = "[Pipeline] Custom Model Processor")]
    public class CustomModelProcessor : ModelProcessor
    {
        bool mCreateBounds = true;
        bool mCreatePicking = false;

        // Effect to apply to the model.
        string mEffectName;

        // Member variables used in constructing tag data.
        List<Vector3> mVertices = new List<Vector3>();
        List<int> mIndices = new List<int>();
        BoundingSphere mBoundingSphere;

        /// <summary>
        /// Processes the Model.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            ModelContent model = base.Process(input, context);

            // This method adds all of the vertex position data to a new list, in order
            // to construct triangle-picking data for use in-game, as well as bounding
            // volumes for collision purposes. As not all models will need this, it is
            // optional, determined by the CreatePickingData and CreateBounds properties.
            FindVertices(input);

            // Create a new dictionary to store tag data inside of. This can also be used
            // within the game, rather than just in the Content Pipeline; it's a clever
            // means of passing information from this project to another without parameters.
            Dictionary<string, object> tagData = new Dictionary<string, object>();
            model.Tag = tagData;

            // If CreatePickingData is set to true in the editor, store vertex and index 
            // triangle-picking data in the Tag as two lists of Vector3 and int.
            if (CreatePickingData)
            {
                tagData.Add("Vertices", mVertices.ToArray());
                tagData.Add("Indices", mIndices.ToArray());
            }

            // If CreateBounds is set to true in the editor, store both a BoundingSphere
            // and a BoundingBox object in the Tag dictionary.
            if (CreateBounds)
            {
                mBoundingSphere = BoundingSphere.CreateFromPoints(mVertices);
                tagData.Add("BoundingSphere", mBoundingSphere);
                tagData.Add("BoundingBox", BoundingBox.CreateFromSphere(mBoundingSphere));
            }

            return model;
        }

        protected void FindVertices(NodeContent inNode)
        {
            // Is this a node or a mesh?
            MeshContent mesh = inNode as MeshContent;

            if (mesh != null)
            {
                // Look up the absolute transform of the mesh.
                Matrix absoluteTransform = mesh.AbsoluteTransform;

                // Loop over all pieces of geometry in the mesh and look through the indices. Every
                // group of three indices represents one triangle. We then lok up the position of the
                // vertex and store it in the list, which is then saved as MeshTag data.
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    foreach (int index in geometry.Indices)
                    {
                        Vector3 vertex = geometry.Vertices.Positions[index];

                        vertex = Vector3.Transform(vertex, absoluteTransform);

                        mVertices.Add(vertex);
                        mIndices.Add(index);
                    }
                }
            }

            foreach (NodeContent child in inNode.Children)
            {
                FindVertices(child);
            }
        }

        /// <summary>
        /// Loads the specified effect into the material.
        /// </summary>
        protected override MaterialContent ConvertMaterial(MaterialContent material,
                                                           ContentProcessorContext context)
        {
            EffectMaterialContent newMaterial = new EffectMaterialContent();

            if (string.IsNullOrWhiteSpace(mEffectName))
            {
                throw new ArgumentNullException("Effect asset undefined.");
            }

            newMaterial.Effect = new ExternalReference<EffectContent>(mEffectName);

            foreach (KeyValuePair<string,
                     ExternalReference<TextureContent>> pair in material.Textures)
            {
                newMaterial.Textures.Add(pair.Key, pair.Value);
            }

            return context.Convert<MaterialContent,
                                   MaterialContent>(newMaterial,
                                   typeof(MaterialProcessor).Name);
        }

        /// <summary>
        /// Default effect.
        /// </summary>
        [Browsable(false)]
        public override MaterialProcessorDefaultEffect DefaultEffect
        {
            get { return base.DefaultEffect; }
            set { base.DefaultEffect = value; }
        }

        /// <summary>
        /// String containing the name of the effect asset to load.
        /// </summary>
        [Browsable(true)]
        public string EffectAsset
        {
            get { return mEffectName; }
            set
            {
                mEffectName = value;
            }
        }

        /// <summary>
        /// Determines whether the Model Processor creates triangle-picking data for
        /// the model and adds it to the model's Tag data. Set to false by default.
        /// </summary>
        [DisplayName("Create Picking Data"), DefaultValue(false), Browsable(true)]
        public bool CreatePickingData
        {
            get { return mCreatePicking; }
            set
            {
                mCreatePicking = value;
            }
        }

        /// <summary>
        /// Determines whether the Model Processor creates BoundingBox and BoundingSphere
        /// data for the model and adds it to the model's Tag data. Set to true by default.
        /// </summary>
        [DisplayName("Create Bounds"), DefaultValue(true), Browsable(true)]
        public bool CreateBounds
        {
            get { return mCreateBounds; }
            set
            {
                mCreateBounds = value;
            }
        }
    }
}
