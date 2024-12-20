using UnityEngine;

namespace Nine.AssetReferences.Editor
{
    public static class RectExtensions
    {
        public static Rect SetWidth(this Rect rect, float width)
        {
            rect.width = width;

            return rect;
        }

        public static Rect SetHeight(this Rect rect, float height)
        {
            rect.height = height;

            return rect;
        }

        public static Rect SetSize(this Rect rect, float width, float height)
        {
            rect.width = width;
            rect.height = height;

            return rect;
        }

        public static Rect SetSize(this Rect rect, Vector2 size)
        {
            rect.size = size;

            return rect;
        }

        public static Rect HorizontalPadding(this Rect rect, float padding)
        {
            rect.x += padding;
            rect.width -= padding * 2f;

            return rect;
        }

        public static Rect HorizontalPadding(this Rect rect, float left, float right)
        {
            rect.x += left;
            rect.width -= left + right;

            return rect;
        }

        public static Rect VerticalPadding(this Rect rect, float padding)
        {
            rect.y += padding;
            rect.height -= padding * 2f;

            return rect;
        }

        public static Rect VerticalPadding(this Rect rect, float top, float bottom)
        {
            rect.y += top;
            rect.height -= top + bottom;

            return rect;
        }

        public static Rect Padding(this Rect rect, float padding)
        {
            rect.position += new Vector2(padding, padding);
            rect.size -= new Vector2(padding, padding) * 2f;

            return rect;
        }

        public static Rect Padding(this Rect rect, float horizontal, float vertical)
        {
            rect.position += new Vector2(horizontal, vertical);
            rect.size -= new Vector2(horizontal, vertical) * 2f;

            return rect;
        }

        public static Rect Split(this Rect rect, int index, int count)
        {
            var num = rect.width / count;
            rect.width = num;
            rect.x += num * index;

            return rect;
        }

        public static Rect SplitVertical(this Rect rect, int index, int count)
        {
            var num = rect.height / count;
            rect.height = num;
            rect.y += num * index;

            return rect;
        }

        public static Rect SetCenterX(this Rect rect, float x)
        {
            rect.center = new Vector2(x, rect.center.y);

            return rect;
        }

        public static Rect SetCenterY(this Rect rect, float y)
        {
            rect.center = new Vector2(rect.center.x, y);

            return rect;
        }

        public static Rect SetCenter(this Rect rect, float x, float y)
        {
            rect.center = new Vector2(x, y);

            return rect;
        }

        public static Rect SetCenter(this Rect rect, Vector2 center)
        {
            rect.center = center;

            return rect;
        }

        public static Rect SetPosition(this Rect rect, Vector2 position)
        {
            rect.position = position;

            return rect;
        }

        public static Rect SetMin(this Rect rect, Vector2 min)
        {
            rect.min = min;

            return rect;
        }


        public static Rect SetMax(this Rect rect, Vector2 max)
        {
            rect.max = max;

            return rect;
        }

        public static Rect SetXMin(this Rect rect, float xMin)
        {
            rect.xMin = xMin;

            return rect;
        }

        public static Rect SetYMin(this Rect rect, float yMin)
        {
            rect.yMin = yMin;

            return rect;
        }

        public static Rect SetXMax(this Rect rect, float xMax)
        {
            rect.xMax = xMax;

            return rect;
        }

        public static Rect SetYMax(this Rect rect, float yMax)
        {
            rect.yMax = yMax;

            return rect;
        }
    }
}