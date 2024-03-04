﻿using LaserGRBL;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Core;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace LaserGRBL.Obj3D
{
    public class Object3DVertex
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public Color OriginalColor { get; set; }
        public Color? NewColor { get; set; }
        public int Index { get; set; }
    }

    public class Object3DDisplayList
    {
        public DisplayList DisplayList { get; private set; }
        public List<Object3DVertex> Vertices { get; } = new List<Object3DVertex>();
        public int MinIndex { get; private set; } = int.MaxValue;
        public int MaxIndex { get; private set; } = int.MinValue;
        public void SetIndex(int index)
        {
            if (index < MinIndex) MinIndex = index;
            if (index > MaxIndex) MaxIndex = index;
        }

        public void Begin(OpenGL gl, float lineWidth)
        {
            DisplayList = new DisplayList();
            DisplayList.Generate(gl);
            DisplayList.New(gl, DisplayList.DisplayListMode.CompileAndExecute);
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT | OpenGL.GL_LINE_BIT);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.LineWidth(lineWidth);
            //gl.Enable(OpenGL.GL_POLYGON_SMOOTH);

            /*
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            gl.PushAttrib(OpenGL.GL_CURRENT_BIT | OpenGL.GL_ENABLE_BIT | OpenGL.GL_LINE_BIT);
            gl.Disable(OpenGL.GL_LIGHTING);
            gl.Disable(OpenGL.GL_TEXTURE_2D);
            gl.Enable(OpenGL.GL_POLYGON_SMOOTH);
            gl.ClearDepth(1.0f);
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            gl.DepthFunc(OpenGL.GL_LEQUAL);
            gl.Disable(OpenGL.GL_CULL_FACE);
            gl.Hint(OpenGL.GL_PERSPECTIVE_CORRECTION_HINT, OpenGL.GL_NICEST);
            gl.Enable(OpenGL.GL_BLEND);
            gl.BlendFunc(OpenGL.GL_SRC_ALPHA, OpenGL.GL_ONE_MINUS_SRC_ALPHA);
            gl.Enable(OpenGL.GL_LINE_SMOOTH);
            gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);
            gl.PolygonMode(FaceMode.FrontAndBack, PolygonMode.Filled);
            */
            //gl.Enable(OpenGL.GL_LINE_SMOOTH);
            //gl.Hint(OpenGL.GL_LINE_SMOOTH_HINT, OpenGL.GL_NICEST);
            gl.Begin(OpenGL.GL_LINES);
        }

        public void End(OpenGL gl)
        {
            //gl.Disable(OpenGL.GL_POLYGON_SMOOTH);
            gl.End();
            gl.PopAttrib();
            DisplayList.End(gl);
        }

    }

    public abstract class Object3D : SceneElement, IRenderable
    {
        [XmlIgnore]
        protected List<Object3DDisplayList> mDisplayLists = new List<Object3DDisplayList>();
        [XmlIgnore]
        protected float mLineWidth;
        [XmlIgnore]
        protected OpenGL mGL;
        [XmlIgnore]
        protected Object3DDisplayList mCurrentDisplayList = null;
        [XmlIgnore]
        protected const int MAX_VECTOR_IN_DISPLAY_LIST = 10000;

        public Object3D(string name, float lineWidth)
        {
            Name = name;
            mLineWidth = lineWidth;
        }

        protected virtual void ClearDisplayList()
        {
            foreach (Object3DDisplayList list in mDisplayLists)
            {
                list.DisplayList.Delete(mGL);
            }
            mDisplayLists.Clear();
        }

        public virtual void Render(OpenGL gl, RenderMode renderMode)
        {
            if (renderMode == RenderMode.Design)
            {
                if (mDisplayLists.Count <= 0)
                {
                    mGL = gl;
                    CreateDisplayList();
                }
                else
                {
                    CallDisplayList(gl);
                }
            }
        }

        protected virtual void CallDisplayList(OpenGL gl)
        {
            foreach (Object3DDisplayList object3DDisplayList in mDisplayLists)
            {
                object3DDisplayList.DisplayList.Call(gl);
            }
        }

        private void CreateDisplayList()
        {
            mCurrentDisplayList = new Object3DDisplayList();
            mCurrentDisplayList.Begin(mGL, mLineWidth);
            mDisplayLists.Add(mCurrentDisplayList);
            Draw();
            mCurrentDisplayList.End(mGL);
            mCurrentDisplayList = null;
        }

        protected abstract void Draw();

        protected void NewDisplayList()
        {
            mCurrentDisplayList.End(mGL);
            mCurrentDisplayList = new Object3DDisplayList();
            mCurrentDisplayList.Begin(mGL, mLineWidth);
            mDisplayLists.Add(mCurrentDisplayList);
        }

        public void AddVertex(double x, double y, double z, Color color, int index = -1)
        {
            mGL.Color(color.R, color.G, color.B, color.A);
            mGL.Vertex(x, y, z);
            mCurrentDisplayList.Vertices.Add(new Object3DVertex { X = x, Y = y, Z = z, OriginalColor = color, Index = index });
            mCurrentDisplayList.SetIndex(index);
        }

        public void CheckListSize()
        {
            if (mCurrentDisplayList.Vertices.Count > MAX_VECTOR_IN_DISPLAY_LIST) NewDisplayList();
        }

    }

    public class Grid3D : Object3D
    {
        [XmlIgnore]
        public int MaxWidth { get; set; } = 2000;
        [XmlIgnore]
        public int MaxHeight { get; set; } = 2000;
        [XmlIgnore]
        private DisplayList mDisplayOrigins;
        [XmlIgnore]
        private DisplayList mDisplayTick;
        [XmlIgnore]
        private DisplayList mDisplayMinor;
        [XmlIgnore]
        public bool ShowMinor { get; set; }
        [XmlIgnore]
        private Color mTicksColor = new Color();
        [XmlIgnore]
        public Color TicksColor {
            get => mTicksColor;
            set
            {
                if (value != mTicksColor)
                {
                    mTicksColor = value;
                    ClearDisplayList();
                }
            }
        }
        [XmlIgnore]
        private Color mMinorsColor = new Color();
        [XmlIgnore]
        public Color MinorsColor
        {
            get => mMinorsColor;
            set
            {
                if (value != mMinorsColor)
                {
                    mMinorsColor = value;
                    ClearDisplayList();
                }
            }
        }
        [XmlIgnore]
        private Color mOriginsColor = new Color();
        [XmlIgnore]
        public Color OriginsColor
        {
            get => mOriginsColor;
            set
            {
                if (value != mOriginsColor)
                {
                    mOriginsColor = value;
                    ClearDisplayList();
                }
            }
        }


        public Grid3D() : base("Grid", 0.01f) { }

        private void DrawCross(float position, GLColor color, float z)
        {
            AddVertex(position, -MaxHeight, z, color);
            AddVertex(position, MaxHeight, z, color);
            AddVertex(-MaxHeight, position, z, color);
            AddVertex(MaxHeight, position, z, color);
        }

        protected override void Draw()
        {
            // minor tick display list
            mDisplayMinor = mCurrentDisplayList.DisplayList;
            for (int i = -MaxWidth; i <= MaxWidth; i++)
            {
                if (i % 10 == 0 || i == 0) continue;
                DrawCross(i, MinorsColor, -0.3f);
            }
            // DCA tick display list
            NewDisplayList();
            mDisplayTick = mCurrentDisplayList.DisplayList;
            for (int i = -MaxWidth; i <= MaxWidth; i++)
            {
                if (i % 10 != 0 || i == 0) continue;
                DrawCross(i, TicksColor, -0.2f);
            }
            // oringins display list
            NewDisplayList();
            mDisplayOrigins = mCurrentDisplayList.DisplayList;
            DrawCross(0, OriginsColor, -0.1f);
        }

        protected override void CallDisplayList(OpenGL gl)
        {
            mDisplayOrigins.Call(gl);
            mDisplayTick.Call(gl);
            if (ShowMinor)
            {
                mDisplayMinor.Call(gl);
            }
        }

    }

    public class Grbl3D : Object3D
    {

        [XmlIgnore]
        public readonly GrblFile File;
        [XmlIgnore]
        private readonly bool _justLaserOffMovements;

        public Object3DVertex CurrentVertex = null;

        public Grbl3D(GrblFile file, string name, bool justLaserOffMovements) : base(name, 1f)
        {
            File = file;
            _justLaserOffMovements = justLaserOffMovements;
        }

        protected override void Draw()
        {
            File.To3D(this, _justLaserOffMovements, 0.1f);
        }

        private void ReplaceColor(Color? color, int startIndex, int stopIndex)
        {
            List<Object3DDisplayList> invalidateList = new List<Object3DDisplayList>();
            for (int i = 0; i < mDisplayLists.Count; i++)
            {
                Object3DDisplayList list = mDisplayLists[i];
                if (startIndex <= list.MaxIndex && stopIndex >= list.MinIndex)
                {
                    invalidateList.Add(list);
                }
                if (stopIndex < list.MinIndex) break;
            }
            if (invalidateList.Count > 0)
            {
                foreach (Object3DDisplayList list in invalidateList)
                {
                    list.DisplayList.Delete(mGL);
                    list.Begin(mGL, mLineWidth);
                    foreach (Object3DVertex vertex in list.Vertices)
                    {
                        if (vertex.Index >= startIndex && vertex.Index <= stopIndex)
                        {
                            Color newColor = new Color();
                            if (color != null)
                            {
                                Color setColor = (Color)color;
                                newColor = Color.FromArgb(
                                    setColor.A > vertex.OriginalColor.A ? setColor.A : vertex.OriginalColor.A,
                                    setColor.R > vertex.OriginalColor.R ? setColor.R : vertex.OriginalColor.R,
                                    setColor.G > vertex.OriginalColor.G ? setColor.G : vertex.OriginalColor.G,
                                    setColor.B > vertex.OriginalColor.B ? setColor.B : vertex.OriginalColor.B
                                );
                            }
                            else
                            {
                                newColor = Color.FromArgb(vertex.OriginalColor.A, vertex.OriginalColor);
                            }
                            vertex.NewColor = newColor;
                            CurrentVertex = vertex;
                            mGL.Color(newColor);
                            mGL.Vertex(vertex.X, vertex.Y, vertex.Z);
                        }
                        else
                        {
                            mGL.Color(vertex.NewColor != null ? vertex.NewColor : vertex.OriginalColor);
                            mGL.Vertex(vertex.X, vertex.Y, vertex.Z);
                        }
                    }
                    list.End(mGL);
                }
            }
        }

        public void OverrideColor(Color color, int startIndex, int stopIndex)
        {
            ReplaceColor(color, startIndex, stopIndex);
        }

        public void ResetColor()
        {
            foreach (Object3DDisplayList list in mDisplayLists)
            {
                foreach (Object3DVertex vertex in list.Vertices)
                {
                    vertex.NewColor = null;
                }
            }
            ReplaceColor(null, int.MinValue, int.MaxValue);
        }

    }

}
