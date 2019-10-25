using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Elements.Geometry
{
    /// <summary>
    /// A right-handed coordinate system defined by an origin, x, y, and z axes.
    /// </summary>
    /// <example>
    /// [!code-csharp[Main](../../test/Elements.Tests/Examples/TransformExample.cs?name=example)]
    /// </example>
    public partial class Transform
    {
        private Matrix _matrix;

        /// <summary>
        /// Create the identity transform.
        /// </summary>
        public Transform()
        {
            this._matrix = new Matrix();
            SetComponentsFromMatrix();
        }
        
        /// <summary>
        /// Create a transform by copying another transform.
        /// </summary>
        /// <param name="t">The transform to copy.</param>
        public Transform(Transform t): 
            this(new Matrix(new Vector3(t.XAxis), new Vector3(t.YAxis), new Vector3(t.ZAxis), new Vector3(t.Origin))){}

        /// <summary>
        /// Create a transform with a translation.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="rotation">An optional rotation in degrees around the transform's z axis.</param>
        public Transform(Vector3 origin, double rotation = 0.0)
        {
            this._matrix = new Matrix();
            this._matrix.SetupTranslation(origin);
            ApplyRotationAndTranslation(rotation, this._matrix.ZAxis, Vector3.Origin);
            
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Create a transform with a translation.
        /// </summary>
        /// <param name="x">The X component of translation.</param>
        /// <param name="y">The Y component of translation.</param>
        /// <param name="z">The Z component of translation.</param>
        /// <param name="rotation">An optional rotation in degrees around the transform's z axis.</param>
        public Transform(double x, double y, double z, double rotation = 0.0)
        {
            this._matrix = new Matrix();
            this._matrix.SetupTranslation(new Vector3(x, y, z));
            ApplyRotationAndTranslation(rotation, this._matrix.ZAxis, Vector3.Origin);
            
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Create a transform centered at origin with its Z axis pointing
        /// along up.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="z">The vector which will define the Z axis of the transform.</param>
        /// <param name="rotation">An optional rotation around the z axis.</param>
        public Transform(Vector3 origin, Vector3 z, double rotation = 0.0)
        {
            Vector3 x = Vector3.XAxis;
            Vector3 y = Vector3.YAxis;

            if(!z.IsParallelTo(Vector3.ZAxis))
            {   
                // Project up onto the ortho plane
                var p = new Plane(origin, z);
                var test = Vector3.ZAxis.Project(p);
                x = test.Cross(z);
                y = x.Cross(z.Negate()); 
            }
            
            this._matrix = new Matrix(x, y, z, Vector3.Origin);
            ApplyRotationAndTranslation(rotation, z, origin);

            SetComponentsFromMatrix();
        }

        private void ApplyRotationAndTranslation(double rotation, Vector3 axis, Vector3 translation)
        {
            if(rotation != 0.0)
            {
                this.Rotate(axis, rotation);
            }
            // Apply translation after rotation.
            this.Move(translation);
        }

        /// <summary>
        /// Create a transform by origin and X and Z axes.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="xAxis">The X axis of the transform.</param>
        /// <param name="zAxis">The Z axis of the transform.</param>
        /// <param name="rotation">An optional rotation in degrees around the transform's z axis.</param>
        public Transform(Vector3 origin, Vector3 xAxis, Vector3 zAxis, double rotation = 0.0)
        {
            var x = xAxis.Normalized();
            var z = zAxis.Normalized();
            var y = z.Cross(x).Normalized();
            this._matrix = new Matrix(x, y, z, Vector3.Origin);
            ApplyRotationAndTranslation(rotation, z, origin);

            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Create a transform by a matrix.
        /// </summary>
        /// <param name="matrix">The transform's Matrix.</param>
        
        public Transform(Matrix matrix)
        {
            this._matrix = matrix;
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Create a transform by origin, X, Y, and Z axes.
        /// </summary>
        /// <param name="origin">The origin of the transform.</param>
        /// <param name="xAxis">The X axis of the transform.</param>
        /// <param name="yAxis">The Y axis of the transform.</param>
        /// <param name="zAxis">The Z axis of the transform.</param>
        [JsonConstructor]
        public Transform(Vector3 origin, Vector3 xAxis, Vector3 yAxis, Vector3 zAxis)
        {
            this._matrix = new Matrix(xAxis, yAxis, zAxis, origin);
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Set the components of this transform from a matrix.
        /// Normally, we would pass through the matrix's axis and origin values
        /// to the property getters. Because Transforms base declaration is 
        /// generated though, we only have {get;set;}, so this method is used
        /// after every matrix construction to set the components explicitly.
        /// </summary>
        private void SetComponentsFromMatrix()
        {
            this.Origin = this._matrix.Translation;
            this.XAxis = this._matrix.XAxis;
            this.YAxis = this._matrix.YAxis;
            this.ZAxis = this._matrix.ZAxis;
        }

        /// <summary>
        /// Get a string representation of the transform.
        /// </summary>
        /// <returns>A string representation of the transform.</returns>
        public override string ToString()
        {
            return this._matrix.ToString();
        }

        /// <summary>
        /// Transform a vector into the coordinate space defined by this transform ignoring the translation.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <returns>A new vector transformed by this transform.</returns>
        public Vector3 OfPoint(Vector3 vector)
        {
            return vector * this._matrix;
        }

        /// <summary>
        /// Transform a vector into the coordinate space defined by this transform.
        /// </summary>
        /// <param name="vector">The vector to transform.</param>
        /// <returns>A new vector transformed by this transform.</returns>
        public Vector3 OfVector(Vector3 vector)
        {
            var m = new Matrix(this.XAxis, this.YAxis, this.ZAxis, Vector3.Origin);
            return new Vector3(
                vector.X*m.XAxis.X + vector.Y*YAxis.X + vector.Z*ZAxis.X,
                vector.X*m.XAxis.Y + vector.Y*YAxis.Y + vector.Z*ZAxis.Y,
                vector.X*m.XAxis.Z + vector.Y*YAxis.Z + vector.Z*ZAxis.Z
            );
        }

        /// <summary>
        /// Transform the specified polygon.
        /// </summary>
        /// <param name="polygon">The polygon to transform.</param>
        /// <returns>A new polygon transformed by this transform.</returns>
        public Polygon OfPolygon(Polygon polygon)
        {
            var transformed = new Vector3[polygon.Vertices.Count];
            for (var i = 0; i < transformed.Length; i++)
            {
                transformed[i] = OfPoint(polygon.Vertices[i]);
            }
            var p = new Polygon(transformed);
            return p;
        }

        /// <summary>
        /// Transform the specified polygons.
        /// </summary>
        /// <param name="polygons">The polygons to transform.</param>
        /// <returns>An array of polygons transformed by this transform.</returns>
        public Polygon[] OfPolygons(IList<Polygon> polygons)
        {
            var result = new Polygon[polygons.Count];
            for(var i=0; i<polygons.Count; i++)
            {
                result[i] = OfPolygon(polygons[i]);
            }
            return result;
        }

        /// <summary>
        /// Transform the specified line.
        /// </summary>
        /// <param name="line">The line to transform.</param>
        /// <returns>A new line transformed by this transform.</returns>
        public Line OfLine(Line line)
        {
            return new Line(OfPoint(line.Start), OfPoint(line.End));
        }

        /// <summary>
        /// Transform the specified plane.
        /// </summary>
        /// <param name="plane">The plane to transform.</param>
        /// <returns>A new plane transformed by this transform.</returns>
        public Plane OfPlane(Plane plane)
        {
            return new Plane(OfPoint(plane.Origin), OfVector(plane.Normal));
        }

        /// <summary>
        /// Transform the specified profile.
        /// </summary>
        /// <param name="profile">The profile to transform.</param>
        /// <returns>A new profile transformed by this transform.</returns>
        public Profile OfProfile(Profile profile)
        {
            Polygon[] voids = null;
            if (profile.Voids != null)
            {
                voids = new Polygon[profile.Voids.Count];
                for (var i = 0; i < voids.Length; i++)
                {
                    voids[i] = OfPolygon(profile.Voids[i]);
                }
            }
            var p = new Profile(OfPolygon(profile.Perimeter), voids);
            return p;
        }

        /// <summary>
        /// Concatenate the transform.
        /// </summary>
        /// <param name="transform"></param>
        public void Concatenate(Transform transform)
        {
            this._matrix = this._matrix * transform._matrix;
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Invert this transform.
        /// </summary>
        public void Invert()
        {
            this._matrix = this._matrix.Inverse();
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Apply a translation to the transform.
        /// </summary>
        /// <param name="translation">The translation to apply.</param>
        public void Move(Vector3 translation)
        {
            var m = new Matrix();
            m.SetupTranslation(translation);
            this._matrix = this._matrix * m;
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Apply a rotation to the transform.
        /// </summary>
        /// <param name="axis">The axis of rotation.</param>
        /// <param name="angle">The angle of rotation in degrees.</param>
        public void Rotate(Vector3 axis, double angle)
        {
            var m = new Matrix();
            m.SetupRotate(axis, angle * (Math.PI / 180.0));
            this._matrix = this._matrix * m;
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Apply a scale to the transform.
        /// </summary>
        /// <param name="amount">The amount to scale.</param>
        public void Scale(Vector3 amount)
        {
            var m = new Matrix();
            m.SetupScale(amount);
            this._matrix = this._matrix * m;
            SetComponentsFromMatrix();
        }

        /// <summary>
        /// Calculate XY plane of the transform.
        /// </summary>
        public Plane XY()
        {
            return new Plane(this.Origin, this.ZAxis);
        }

        /// <summary>
        /// Calculate the YZ plane of the transform.
        /// </summary>
        public Plane YZ()
        {
            return new Plane(this.Origin, this.XAxis);
        }

        /// <summary>
        /// Calculate the XZ plane of the transform.
        /// </summary>
        public Plane XZ()
        {
            return new Plane(this.Origin, this.YAxis);
        }
    }
}