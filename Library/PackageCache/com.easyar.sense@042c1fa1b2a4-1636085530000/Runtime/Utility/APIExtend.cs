//================================================================================================================================
//
//  Copyright (c) 2015-2021 VisionStar Information Technology (Shanghai) Co., Ltd. All Rights Reserved.
//  EasyAR is the registered trademark or trademark of VisionStar Information Technology (Shanghai) Co., Ltd in China
//  and other countries for the augmented reality technology developed by VisionStar Information Technology (Shanghai) Co., Ltd.
//
//================================================================================================================================

using UnityEngine;

namespace easyar
{
    /// <summary>
    /// <para xml:lang="en">Extend EasyAR Sense API and Unity API for convenience to do operations like data conversion.</para>
    /// <para xml:lang="zh">扩展EasyAR Sense API 及 Unity API，为数据转换等操作提供便利。</para>
    /// </summary>
    public static class APIExtend
    {
        /// <summary>
        /// <para xml:lang="en">Convert <see cref="Matrix44F"/> to <see cref="Matrix4x4"/>.</para>
        /// <para xml:lang="zh">将<see cref="Matrix44F"/>转成<see cref="Matrix4x4"/>。</para>
        /// </summary>
        public static Matrix4x4 ToUnityMatrix(this Matrix44F matrix44F)
        {
            var matrix4x4 = new Matrix4x4();
            matrix4x4.SetRow(0, new Vector4(matrix44F.data_0, matrix44F.data_1, matrix44F.data_2, matrix44F.data_3));
            matrix4x4.SetRow(1, new Vector4(matrix44F.data_4, matrix44F.data_5, matrix44F.data_6, matrix44F.data_7));
            matrix4x4.SetRow(2, new Vector4(matrix44F.data_8, matrix44F.data_9, matrix44F.data_10, matrix44F.data_11));
            matrix4x4.SetRow(3, new Vector4(matrix44F.data_12, matrix44F.data_13, matrix44F.data_14, matrix44F.data_15));
            return matrix4x4;
        }

        /// <summary>
        /// <para xml:lang="en">Convert <see cref="Matrix4x4"/> to <see cref="Matrix44F"/>.</para>
        /// <para xml:lang="zh">将<see cref="Matrix4x4"/>转成<see cref="Matrix44F"/>。</para>
        /// </summary>
        public static Matrix44F ToEasyARMatrix(this Matrix4x4 matrix4x4)
        {
            return new Matrix44F(
                matrix4x4[0, 0], matrix4x4[0, 1], matrix4x4[0, 2], matrix4x4[0, 3],
                matrix4x4[1, 0], matrix4x4[1, 1], matrix4x4[1, 2], matrix4x4[1, 3],
                matrix4x4[2, 0], matrix4x4[2, 1], matrix4x4[2, 2], matrix4x4[2, 3],
                matrix4x4[3, 0], matrix4x4[3, 1], matrix4x4[3, 2], matrix4x4[3, 3]
                );
        }

        /// <summary>
        /// <para xml:lang="en">Convert <see cref="Matrix44F"/> to <see cref="Pose"/> and transform to Unity coordinate system. <paramref name="pose"/> must represent a <see cref="Pose"/>.</para>
        /// <para xml:lang="zh">将<see cref="Matrix44F"/>转成<see cref="Pose"/>并同时转换到Unity坐标系。<paramref name="pose"/>必须表示一个<see cref="Pose"/>。</para>
        /// </summary>
        public static Pose ToUnityPose(this Matrix44F pose)
        {
            var q = new Quaternion
            {
                w = Mathf.Sqrt(Mathf.Max(0, 1 + pose.data_0 + pose.data_5 + pose.data_10)) / 2,
                x = Mathf.Sqrt(Mathf.Max(0, 1 + pose.data_0 - pose.data_5 - pose.data_10)) / 2,
                y = Mathf.Sqrt(Mathf.Max(0, 1 - pose.data_0 + pose.data_5 - pose.data_10)) / 2,
                z = Mathf.Sqrt(Mathf.Max(0, 1 - pose.data_0 - pose.data_5 + pose.data_10)) / 2
            };

            q.x *= Mathf.Sign(q.x * (pose.data_9 - pose.data_6));
            q.y *= Mathf.Sign(q.y * (pose.data_2 - pose.data_8));
            q.z *= Mathf.Sign(q.z * (pose.data_4 - pose.data_1));

            return new Pose(new Vector3 { x = pose.data_3, y = pose.data_7, z = -pose.data_11 }, new Quaternion { w = q.w, x = -q.x, y = -q.y, z = q.z });
        }

        /// <summary>
        /// <para xml:lang="en">Convert <see cref="Pose"/> to <see cref="Matrix44F"/> and transform to EasyAR coordinate system.</para>
        /// <para xml:lang="zh">将<see cref="Pose"/>转成<see cref="Matrix44F"/>并同时转换到EasyAR坐标系。</para>
        /// </summary>
        public static Matrix44F ToEasyARPose(this Pose pose)
        {
            var p = pose.position;
            var q = pose.rotation;
            return Matrix4x4.TRS(new Vector3 { x = p.x, y = p.y, z = -p.z }, new Quaternion { w = q.w, x = -q.x, y = -q.y, z = q.z }, Vector3.one).ToEasyARMatrix();
        }

        /// <summary>
        /// <para xml:lang="en">Convert <see cref="Vector2"/> to <see cref="Vec2F"/>.</para>
        /// <para xml:lang="zh">将<see cref="Vector2"/>转成<see cref="Vec2F"/>。</para>
        /// </summary>
        public static Vec2F ToEasyARVector(this Vector2 vec2)
        {
            return new Vec2F(vec2.x, vec2.y);
        }

        /// <summary>
        /// <para xml:lang="en">Convert <see cref="Vector3"/> to <see cref="Vec3F"/>.</para>
        /// <para xml:lang="zh">将<see cref="Vector3"/>转成<see cref="Vec3F"/>。</para>
        /// </summary>
        public static Vec3F ToEasyARVector(this Vector3 vec3)
        {
            return new Vec3F(vec3.x, vec3.y, vec3.z);
        }

        /// <summary>
        /// <para xml:lang="en">Convert <see cref="Vec2F"/> to <see cref="Vector2"/>.</para>
        /// <para xml:lang="zh">将<see cref="Vec2F"/>转成<see cref="Vector2"/>。</para>
        /// </summary>
        public static Vector2 ToUnityVector(this Vec2F vec2)
        {
            return new Vector2(vec2.data_0, vec2.data_1);
        }

        /// <summary>
        /// <para xml:lang="en">Convert <see cref="Vec3F"/> to <see cref="Vector3"/>.</para>
        /// <para xml:lang="zh">将<see cref="Vec3F"/>转成<see cref="Vector3"/>。</para>
        /// </summary>
        public static Vector3 ToUnityVector(this Vec3F vec3)
        {
            return new Vector3(vec3.data_0, vec3.data_1, vec3.data_2);
        }

        internal static Pose Inverse(this Pose pose)
        {
            return new Pose
            {
                position = Quaternion.Inverse(pose.rotation) * (-pose.position),
                rotation = Quaternion.Inverse(pose.rotation)
            };
        }

        internal static Pose FlipX(this Pose pose, bool flip)
        {
            if (!flip)
            {
                return pose;
            }
            return new Pose
            {
                position = new Vector3(-pose.position.x, pose.position.y, pose.position.z),
                rotation = new Quaternion(pose.rotation.x, -pose.rotation.y, -pose.rotation.z, pose.rotation.w)
            };
        }
    }
}
