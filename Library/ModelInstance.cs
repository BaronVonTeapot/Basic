using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Library
{
    public class ModelInstance
    {
        /// <summary>
        /// Simple class used to cache data for ModelMesh and ModelMeshPart objects
        /// when multiple effects and materials are to be used on the same model.
        /// </summary>
        public class MeshTag
        {
            public Vector3 Color            { get; set; }
            public Texture2D Texture        { get; set; }
            public float SpecularPower      { get; set; }
            public Effect CachedEffect      { get; set; }
            public Material CachedMaterial  { get; set; }

            /// <summary>
            /// Public constructor.
            /// </summary>
            public MeshTag(Vector3 inColor, Texture2D inTexture, float inSpecularPower)
            {
                this.Color = inColor;
                this.Texture = inTexture;
                this.SpecularPower = inSpecularPower;
            }
        }

        // The model this instance represents.
        Model mModel;
        Matrix[] mBoneTransforms;
        Matrix mTransform, mLocalTransform;
        Material mMaterial;
        Effect mTempEffect;

        // Vectors containing location data.
        Vector3 mPosition, mRotation, mScale;

        // BoundingSphere is typically better than a BoundingBox because
        // it can be used regardless of rotation.
        BoundingSphere mBoundingSphere;

        // Axis-aligned bounding boxes need to be rotated along with their 
        // models, and thus it's often easier to use spherical volumes.
        BoundingBox mBoundingBox;

        // This will be assigned to the Model as the Tag property if
        // the Model was processed using our pipeline.
        Dictionary<string, object> mTagData;

        public ModelInstance(Model inModel, Vector3 inPosition, Vector3 inRotation, Vector3 inScale)
        {
            this.mModel = inModel;
            this.mBoneTransforms = new Matrix[inModel.Bones.Count];
            this.mModel.CopyAbsoluteBoneTransformsTo(mBoneTransforms);

            this.mPosition = inPosition;
            this.mRotation = inRotation;
            this.mScale = inScale;

            this.GenerateBounds();
            this.GenerateTags();
        }

        /// <summary>
        /// Generates the bounding volumes for this instance, either by taking them from
        /// values created in the Content Pipeline or by creating them from scratch.
        /// </summary>
        protected virtual void GenerateBounds()
        {
            // If data has been added to the Tag in the ModelProcessor, we can now
            // cast that data to a dictionary and extract the bounding volumes.
            if (mModel.Tag != null)
            {
                mTagData = (Dictionary<string, object>)mModel.Tag;
                if (mTagData == null)
                {
                    throw new InvalidOperationException
                        ("Model tag data is not set correctly.");
                }

                mBoundingBox = (BoundingBox)mTagData["BoundingBox"];
                mBoundingSphere = (BoundingSphere)mTagData["BoundingSphere"];
            }
            else
            {
                // If the tag-data is null, then we need to create a new bounding
                // sphere at the origin, and merge all of the model's bounding
                // volumes together into a single BoundingSphere.
                BoundingSphere sphere = new BoundingSphere(Vector3.Zero, 0);

                foreach (ModelMesh mesh in mModel.Meshes)
                {
                    BoundingSphere transformed = mesh.BoundingSphere.Transform(
                        mBoneTransforms[mesh.ParentBone.Index]);

                    sphere = BoundingSphere.CreateMerged(sphere, transformed);
                }

                mBoundingSphere = sphere;
                mBoundingBox = BoundingBox.CreateFromSphere(sphere);
            }
        }

        /// <summary>
        /// Draws the model.
        /// </summary>
        public virtual void DrawBasic(GraphicsDevice inGraphicsDevice, Matrix inView,
                                      Matrix inProjection, Vector3 inCameraPosition)
        {
            mTransform = Matrix.CreateScale(mScale) *
                         Matrix.CreateFromYawPitchRoll(mRotation.Y, mRotation.X, mRotation.Z) *
                         Matrix.CreateTranslation(mPosition);

            foreach (ModelMesh modelMesh in this.Model.Meshes)
            {
                Matrix localWorldTransform = mBoneTransforms[modelMesh.ParentBone.Index] *
                                             mTransform;

                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    mTempEffect = meshPart.Effect;
                    if (mTempEffect is BasicEffect)
                    {
                        ((BasicEffect)mTempEffect).World = localWorldTransform;
                        ((BasicEffect)mTempEffect).View = inView;
                        ((BasicEffect)mTempEffect).Projection = inProjection;
                        ((BasicEffect)mTempEffect).EnableDefaultLighting();
                    }
                    else
                    {
                        SetEffectParameter(mTempEffect, "matWorld", localWorldTransform);
                        SetEffectParameter(mTempEffect, "matView", inView);
                        SetEffectParameter(mTempEffect, "matProjection", inProjection);
                        SetEffectParameter(mTempEffect, "CameraPosition", inCameraPosition);
                    }


                    ((MeshTag)meshPart.Tag).CachedMaterial.SetEffectParameters(mTempEffect);

                }

                modelMesh.Draw();
            }
        }

        /// <summary>
        /// Applies a specified value to a named effect parameter.
        /// </summary>
        /// <param name="inEffect">Effect to apply the value to.</param>
        /// <param name="inParameterName">Parameter to set.</param>
        /// <param name="inValue">Parameter value.</param>
        protected virtual void SetEffectParameter(Effect inEffect,
                                                  string inParameterName, object inValue)
        {
            if (inEffect.Parameters[inParameterName] == null) { return; }

            if (inValue is bool) {
                inEffect.Parameters[inParameterName].SetValue((bool)inValue);
            }
            if (inValue is float) {
                inEffect.Parameters[inParameterName].SetValue((float)inValue);
            }
            if (inValue is Vector3) {
                inEffect.Parameters[inParameterName].SetValue((Vector3)inValue);
            }
            if (inValue is Matrix) {
                inEffect.Parameters[inParameterName].SetValue((Matrix)inValue);
            }
            if (inValue is Texture) {
                inEffect.Parameters[inParameterName].SetValue((Texture)inValue);
            }
            if (inValue is Texture2D) {
                inEffect.Parameters[inParameterName].SetValue((Texture2D)inValue);
            }
        }

        /// <summary>
        /// Sets a single parameter on all effects belonging to the model.
        /// </summary>
        /// <param name="inParameterName"></param>
        /// <param name="inValue"></param>
        public virtual void SetSingleParameter(string inParameterName, object inValue)
        {
            foreach (ModelMesh modelMesh in this.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    Effect tempEffect = meshPart.Effect;
                    SetEffectParameter(tempEffect, inParameterName, inValue);
                }
            }
        }

        /// <summary>
        /// Creates a tag object for each ModelMeshPart in the Model, which can
        /// be used to store effects and other useful rendering data.
        /// </summary>
        public virtual void GenerateTags()
        {
            foreach (ModelMesh mesh in this.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    if (meshPart.Effect is BasicEffect)
                    {
                        BasicEffect tempEffect = (BasicEffect)meshPart.Effect;
                        MeshTag meshTag = new MeshTag(tempEffect.DiffuseColor, tempEffect.Texture,
                                                      tempEffect.SpecularPower);
                        meshPart.Tag = meshTag;
                    }
                }
            }
        }

        /// <summary>
        /// Caches a reference to each of the model's current effects.
        /// </summary>
        public virtual void CacheEffects()
        {
            foreach (ModelMesh modelMesh in this.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    ((MeshTag)meshPart.Tag).CachedEffect = meshPart.Effect;
                }
            }
        }

        /// <summary>
        /// Restores effects from the model's cache.
        /// </summary>
        public virtual void RestoreCached()
        {
            foreach (ModelMesh modelMesh in this.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    if (((MeshTag)meshPart.Tag).CachedEffect != null)
                    {
                        meshPart.Effect = ((MeshTag)meshPart.Tag).CachedEffect;
                    }
                }
            }
        }

        /// <summary>
        /// Sets an effect onto all meshes in the model.
        /// </summary>
        /// <param name="inEffect">Effect to apply.</param>
        /// <param name="inCacheEffect">If true, cache the effect.</param>
        public virtual void SetModelEffect(Effect inEffect, bool inCacheEffect)
        {
            foreach (ModelMesh modelMesh in this.Model.Meshes)
            {
                SetMeshEffect(modelMesh.Name, inEffect, inCacheEffect);
            }
        }

        /// <summary>
        /// Sets an effect onto all meshes in the model. Alternative method.
        /// </summary>
        /// <param name="inEffect">Effect to apply.</param>
        /// <param name="inCopyEffect">If true, copy the effect.</param>
        public virtual void SetModelEffectAlt(Effect inEffect, bool inCopyEffect)
        {
            for (int i = 0; i < this.Model.Meshes.Count; i++)
            {
                SetMeshEffect(this.Model.Meshes[i].Name, inEffect, inCopyEffect);
            }
        }


        /// <summary>
        /// Applies an effect to a mesh.
        /// </summary>
        /// <param name="inMeshName">Name of the mesh to apply to.</param>
        /// <param name="inEffect">Effect to apply.</param>
        /// <param name="inCopyEffect">If true, cache the effect.</param>
        protected virtual void SetMeshEffect(string inMeshName, Effect inEffect, bool inCopyEffect)
        {
            foreach (ModelMesh modelMesh in this.Model.Meshes)
            {
                if (modelMesh.Name != inMeshName) { continue; }

                foreach (ModelMeshPart meshPart in modelMesh.MeshParts)
                {
                    Effect effectToSet = inEffect;

                    if (inCopyEffect) { effectToSet = inEffect.Clone(); }
                    MeshTag tag = (MeshTag)meshPart.Tag;

                    if (tag.Texture != null)
                    {
                        SetEffectParameter(effectToSet, "t_Basic", tag.Texture);
                        SetEffectParameter(effectToSet, "TextureEnabled", true);
                    }
                    else
                    {
                        SetEffectParameter(effectToSet, "TextureEnabled", false);
                    }

                    SetEffectParameter(effectToSet, "DiffuseColor", tag.Color);
                    SetEffectParameter(effectToSet, "SpecularColor", tag.SpecularPower);

                    meshPart.Effect = effectToSet;
                }
            }
        }

        /// <summary>
        /// Store a Material in the temporary cache for later use.
        /// </summary>
        /// <param name="inMaterial">Material to cache.</param>
        public void SetModelMaterial(Material inMaterial)
        {
            foreach (ModelMesh modelMesh in this.Model.Meshes)
            {
                this.SetMeshMaterial(modelMesh.Name, inMaterial);
            }
        }

        /// <summary>
        /// Stores a Material in the temporary cache for later use.
        /// </summary>
        /// <param name="inMeshName">Name of the mesh.</param>
        /// <param name="inMaterial">Material to cache.</param>
        protected void SetMeshMaterial(string inMeshName, Material inMaterial)
        {
            foreach (ModelMesh modelMesh in this.Model.Meshes)
            {
                if (inMeshName != modelMesh.Name) { continue; }

                foreach (ModelMeshPart meshPart in modelMesh.MeshParts) {
                    ((MeshTag)meshPart.Tag).CachedMaterial = inMaterial;
                }
            }
        }

        /// <summary>
        /// Model associated with this instance.
        /// </summary>
        public Model Model { get { return mModel; } }

        /// <summary>
        /// BoundingSphere associated with this instance.
        /// </summary>
        public BoundingSphere BoundingSphere
        {
            get
            {
                Matrix tempTransform = Matrix.Multiply(Matrix.CreateScale(mScale),
                                                       Matrix.CreateTranslation(mPosition));
                BoundingSphere mTempSphere = mBoundingSphere;
                mTempSphere = mTempSphere.Transform(tempTransform);
                return mTempSphere;
            }
        }

        /// <summary>
        /// Position vector.
        /// </summary>
        public Vector3 Position
        {
            get { return mPosition; }
            set { mPosition = value; }
        }

        /// <summary>
        /// Rotation values.
        /// </summary>
        public Vector3 Rotation
        {
            get { return mRotation; }
            set { mRotation = value; }
        }

        /// <summary>
        /// Scale.
        /// </summary>
        public Vector3 Scale
        {
            get { return mScale; }
            set { mScale = value; }
        }

        /// <summary>
        /// Array of bone transformations.
        /// </summary>
        public Matrix[] BoneTransforms
        {
            get { return mBoneTransforms; }
        }

        /// <summary>
        /// Material.
        /// </summary>
        public Material Material
        {
            get { return mMaterial; }
            set { mMaterial = value; }
        }
    }
}
