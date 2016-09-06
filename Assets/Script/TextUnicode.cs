/* Unity_Forumより
 * http://forum.unity3d.com/threads/image-fonts-fontawesome.281746/#post-1862245
 */
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;
namespace UnityEngine.UI
{
	public class TextUnicode : Text
	{
		private bool disableDirty = false;
		private Regex regexp = new Regex( @"\\u(?<Value>[a-zA-Z0-9]{4})" );

		#if UNITY_5_3_OR_NEWER
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            string cache = this.text;
            disableDirty = true;
            this.text = this.Decode(this.text);
            base.OnPopulateMesh(toFill);
            this.text = cache;
            disableDirty = false;
        }
		#else
		protected override void OnFillVBO( List<UIVertex> vbo )
		{
			string cache = this.text;
			disableDirty = true;
			this.text = this.Decode( this.text );
			base.OnFillVBO( vbo );
			this.text = cache;
			disableDirty = false;
		}
		#endif

		private string Decode( string value )
		{
			return regexp.Replace( value, m => ( (char) int.Parse( m.Groups["Value"].Value, NumberStyles.HexNumber ) ).ToString() );
		}

		public override void SetLayoutDirty()
		{
			if (disableDirty)
				return;
			base.SetLayoutDirty();
		}

		public override void SetVerticesDirty()
		{
			if (disableDirty)
				return;

			base.SetVerticesDirty();
		}

		public override void SetMaterialDirty()
		{
			if (disableDirty)
				return;

			base.SetMaterialDirty();
		}
	}

}