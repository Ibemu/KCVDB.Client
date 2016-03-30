// http://d.hatena.ne.jp/siokoshou/20070315

using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KCVDB.Client.Clients.Senders.Diff
{
	[ProtoContract]
	public struct DiffResult
	{
		[ProtoMember( 1, DataFormat = DataFormat.TwosComplement )]
		public int OriginalStart;

		[ProtoMember( 2, DataFormat = DataFormat.TwosComplement )]
		public int OriginalLength;

		[ProtoMember( 3 )]
		public string Modified;

		public DiffResult( int orgStart, int orgLength, string modified )
		{
			this.OriginalStart = orgStart;
			this.OriginalLength = orgLength;
			this.Modified = modified;
		}

		public override string ToString()
		{
			return "OrgStart:" + this.OriginalStart + ", OrgLen:" + this.OriginalLength
				+ ", Mod:" + this.Modified;
		}
	}

	public sealed class FastDiff
	{
		private string dataA, dataB;
		private bool isSwap;
		private Snake[] fp;

		private Func<int, int, bool> isSame;


		private FastDiff() { }

		/// <summary>単一行の各文字を比較します</summary>
		/// <param name="textA">元テキスト</param>
		/// <param name="textB">変更テキスト</param>
		/// <returns>比較結果</returns>
		public static IList<DiffResult> DiffChar( string textA, string textB )
		{
			if ( string.IsNullOrEmpty( textA ) || string.IsNullOrEmpty( textB ) )
				return StringNullOrEmpty( textA, textB );

			FastDiff diff = new FastDiff();
			if ( textA.Length <= textB.Length )
			{
				diff.dataA = textA;
				diff.dataB = textB;
			}
			else
			{
				diff.isSwap = true;
				diff.dataA = textB;
				diff.dataB = textA;
			}

			diff.isSame = delegate( int posA, int posB )
			{
				return diff.dataA[ posA ] == diff.dataB[ posB ];
			};

			return diff.DetectDiff();
		}


		private static IList<DiffResult> StringNullOrEmpty( string textA, string textB )
		{
			int lengthA = textA?.Length ?? 0;
			int lengthB = textB?.Length ?? 0;
			return PresentDiff( new CommonSubsequence( lengthA, lengthB, 0, null ), textA, textB );
		}

		private IList<DiffResult> DetectDiff()
		{
			Debug.Assert( this.dataA.Length <= this.dataB.Length );

			this.fp = new Snake[ this.dataA.Length + this.dataB.Length + 3 ];
			int d = this.dataB.Length - this.dataA.Length;
			int p = 0;
			do
			{
				//Debug.Unindent();
				//Debug.WriteLine( "p:" + p );
				//Debug.Indent();

				for ( int k = -p; k < d; k++ )
					SearchSnake( k );

				for ( int k = d + p; k >= d; k-- )
					SearchSnake( k );

				p++;
			}
			while ( this.fp[ this.dataB.Length + 1 ].posB != ( this.dataB.Length + 1 ) );

			// 末尾検出用のCommonSubsequence
			CommonSubsequence endCS = new CommonSubsequence( this.dataA.Length, this.dataB.Length, 0, this.fp[ this.dataB.Length + 1 ].CS );
			CommonSubsequence result = CommonSubsequence.Reverse( endCS );

			if ( this.isSwap )
				return PresentDiffSwap( result, this.dataA, this.dataB );
			else
				return PresentDiff( result, this.dataA, this.dataB );
		}

		private void SearchSnake( int k )
		{
			int kk = this.dataA.Length + 1 + k;
			CommonSubsequence previousCS = null;
			int posA = 0, posB = 0;

			int lk = kk - 1;
			int rk = kk + 1;

			// 論文のfp[n]は-1始まりだが、0始まりのほうが初期化の都合がよいため、
			// +1のゲタを履かせる。fpから読む際は-1し、書く際は+1する。
			int lb = this.fp[ lk ].posB;
			int rb = this.fp[ rk ].posB - 1;

			//Debug.Write( "fp[" + string.Format( "{0,2}", k ) + "]=Snake( " + string.Format( "{0,2}", k )
			//    + ", max( fp[" + string.Format( "{0,2}", ( k - 1 ) ) + "]+1= " + string.Format( "{0,2}", lb )
			//    + ", fp[" + string.Format( "{0,2}", ( k + 1 ) ) + "]= " + string.Format( "{0,2}", rb ) + " ))," );

			if ( lb > rb )
			{
				posB = lb;
				previousCS = this.fp[ lk ].CS;
			}
			else
			{
				posB = rb;
				previousCS = this.fp[ rk ].CS;
			}
			posA = posB - k;

			int startA = posA;
			int startB = posB;

			//Debug.Write( "(x: " + string.Format( "{0,2}", startA ) + ", y: " + string.Format( "{0,2}", startB ) + " )" );

			while ( ( posA < this.dataA.Length )
				&&  ( posB < this.dataB.Length )
				&&  this.isSame( posA, posB ) )
			{
				posA++;
				posB++;
			}

			if ( startA != posA )
			{
				this.fp[ kk ].CS = new CommonSubsequence( startA, startB, posA - startA, previousCS );
			}
			else
			{
				this.fp[ kk ].CS = previousCS;
			}
			this.fp[ kk ].posB = posB + 1; // fpへ+1して書く。論文のfpに+1のゲタを履かせる。

			//Debug.WriteLine( "= " + string.Format( "{0,2}", posB ) );
		}

		/// <summary>結果出力</summary>
		private static IList<DiffResult> PresentDiff( CommonSubsequence cs, string dataA, string dataB )
		{
			List<DiffResult> list = new List<DiffResult>();
			int originalStart = 0, modifiedStart = 0;

			while ( true )
			{
				if (   originalStart < cs.StartA
					|| modifiedStart < cs.StartB )
				{
					DiffResult d = new DiffResult(
						originalStart, cs.StartA - originalStart,
						dataB.Substring( modifiedStart, cs.StartB - modifiedStart ) );
					list.Add( d );
				}

				// 末尾検出
				if ( cs.Length == 0 ) break;

				originalStart = cs.StartA;
				modifiedStart = cs.StartB;

				originalStart += cs.Length;
				modifiedStart += cs.Length;

				cs = cs.Next;
			}
			return list;
		}

		/// <summary>結果出力</summary>
		private static IList<DiffResult> PresentDiffSwap( CommonSubsequence cs, string dataA, string dataB )
		{
			List<DiffResult> list = new List<DiffResult>();
			int originalStart = 0, modifiedStart = 0;

			while ( true )
			{
				if (   originalStart < cs.StartB
					|| modifiedStart < cs.StartA )
				{
					DiffResult d = new DiffResult(
						originalStart, cs.StartB - originalStart,
						dataA.Substring( modifiedStart, cs.StartA - modifiedStart ) );
					list.Add( d );
				}

				// 末尾検出
				if ( cs.Length == 0 ) break;

				originalStart = cs.StartB;
				modifiedStart = cs.StartA;

				originalStart += cs.Length;
				modifiedStart += cs.Length;

				cs = cs.Next;
			}
			return list;
		}

		private struct Snake
		{
			public int posB;
			public CommonSubsequence CS;

			public override string ToString()
			{
				return "posB:" + this.posB + ", CS:" + ( ( this.CS == null ) ? "null" : "exist" );
			}
		}

		private class CommonSubsequence
		{
			private int startA_, startB_;
			private int length_;
			public CommonSubsequence Next;

			public int StartA { get { return this.startA_; } }
			public int StartB { get { return this.startB_; } }
			public int Length { get { return this.length_; } }

			public CommonSubsequence() { }

			public CommonSubsequence( int startA, int startB, int length, CommonSubsequence next )
			{
				this.startA_ = startA;
				this.startB_ = startB;
				this.length_ = length;
				this.Next = next;
			}

			/// <summary>リンクリスト反転</summary>
			public static CommonSubsequence Reverse( CommonSubsequence old )
			{
				CommonSubsequence newTop = null;
				while ( old != null )
				{
					CommonSubsequence next = old.Next;
					old.Next = newTop;
					newTop = old;
					old = next;
				}
				return newTop;
			}

			public override string ToString()
			{
				return "Length:" + this.Length + ", A:" + this.StartA.ToString()
					+ ", B:" + this.StartB.ToString() + ", Next:" + ( ( this.Next == null ) ? "null" : "exist" );
			}
		}

	}
}