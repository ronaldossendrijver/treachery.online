/*
 * Copyright 2020-2021 Ronald Ossendrijver. All rights reserved.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;

namespace Treachery.Shared
{
    public abstract class Segment
    {
        public PointF Start { get; set; }

        public PointF End { get; set; }

        public abstract string ToSvgString();

        public abstract void AddTo(GraphicsPath path);

        public abstract void AddTo(List<PointF> path);

        protected string S(PointF p)
        {
            return string.Format(CultureInfo.InvariantCulture, "new PointF({0}f,{1}f)", p.X, p.Y);
        }

        protected PointF Translate(PointF p, float dX, float dY)
        {
            return new PointF(p.X + dX, p.Y + dY);
        }
    }

    public class BezierTo : Segment
    {
        public BezierTo(PointF start, PointF end, PointF control1, PointF control2)
        {
            Start = start;
            End = end;
            Control1 = control1;
            Control2 = control2;
        }

        public PointF Control1 { get; set; }
        public PointF Control2 { get; set; }

        public string ToLongString()
        {
            return string.Format("new BezierTo({0}, {1}, {2}, {3})", S(Start), S(End), S(Control1), S(Control2));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "B({0}f,{1}f,{2}f,{3}f,{4}f,{5}f,{6}f,{7}f)", Start.X, Start.Y, End.X, End.Y, Control1.X, Control1.Y, Control2.X, Control2.Y);
        }

        public override string ToSvgString()
        {
            return string.Format(CultureInfo.InvariantCulture, "C{0},{1} {2},{3} {4},{5}", Control1.X, Control1.Y, Control2.X, Control2.Y, End.X, End.Y);
        }

        public override void AddTo(GraphicsPath path)
        {
            path.AddBezier(Start, Control1, Control2, End);
        }

        public override void AddTo(List<PointF> path)
        {
            path.Add(End);
        }
    }

    public class MoveTo : Segment
    {
        public MoveTo(PointF start, PointF end)
        {
            Start = start;
            End = end;
        }

        public string ToLongString()
        {
            return string.Format("new MoveTo({0}, {1})", S(Start), S(End));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "M({0}f,{1}f,{2}f,{3}f)", Start.X, Start.Y, End.X, End.Y);
        }

        public override string ToSvgString()
        {
            return string.Format(CultureInfo.InvariantCulture, "M{0},{1}", End.X, End.Y);
        }

        public override void AddTo(GraphicsPath path)
        {
            path.StartFigure();
        }

        public override void AddTo(List<PointF> path)
        {
            path.Add(End);
        }
    }

    public class Close : Segment
    {
        public string ToLongString()
        {
            return string.Format("new Close()");
        }

        public override string ToString()
        {
            return string.Format("C()");
        }

        public override string ToSvgString()
        {
            return "Z";
        }

        public override void AddTo(GraphicsPath path)
        {
            path.CloseFigure();
        }

        public override void AddTo(List<PointF> path)
        {

        }
    }

    public class LineTo : Segment
    {
        public LineTo(PointF start, PointF end)
        {
            Start = start;
            End = end;
        }

        public string ToLongString()
        {
            return string.Format("new LineTo({0}, {1})", S(Start), S(End));
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "L({0}f,{1}f,{2}f,{3}f)", Start.X, Start.Y, End.X, End.Y);
        }

        public override string ToSvgString()
        {
            return string.Format(CultureInfo.InvariantCulture, "L{0},{1}", End.X, End.Y);
        }

        public override void AddTo(GraphicsPath path)
        {
            path.AddLine(Start, End);
        }

        public override void AddTo(List<PointF> path)
        {
            path.Add(End);
        }
    }

    public class Arc : Segment
    {
        public float Radius { get; set; }
        public float StartAngle { get; set; }
        public float EndAngle { get; set; }
        public bool CounterClockwise { get; set; }

        public Arc(PointF center, float radius, float startAngle, float endAngle, bool counterClockwise)
        {
            Start = center;
            End = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
            CounterClockwise = counterClockwise;
        }

        public string ToLongString()
        {
            return string.Format("new Arc({0}, {1}, {2}, {3}, {4})", S(Start), Radius, StartAngle, EndAngle, CounterClockwise);
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public override string ToSvgString()
        {
            throw new NotImplementedException();
        }

        public override void AddTo(GraphicsPath path)
        {
            path.AddArc(Start.X, Start.Y, Radius, Radius, StartAngle, EndAngle);
        }

        public override void AddTo(List<PointF> path)
        {
            path.Add(Translate(Start, -Radius, 0));
            path.Add(Translate(Start, 0, -Radius));
            path.Add(Translate(Start, Radius, 0));
            path.Add(Translate(Start, 0, Radius));
        }
    }

    public class Point<T>
    {
        public T X;
        public T Y;

        public Point(T x, T y)
        {
            X = x;
            Y = y;
        }

        public Point()
        {

        }
    }

    public class Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point()
        {

        }
    }

}
