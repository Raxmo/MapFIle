using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Utils;

namespace MapFile
{
	public struct Chunk
	{
		public byte BiomeID;
		public Vec BiomeCenter;

		public Chunk(byte id, Vec cent)
		{
			BiomeID = id;
			BiomeCenter = cent;
		}

		public string Print()
		{
			return "ID: " + BiomeID + ", Center X: " + BiomeCenter[0] + ", Center Y: " + BiomeCenter[1];
		}
	}

	public struct GlobalData
	{
		/* width -> uint
		 * height -> uint
		 * array of bit data -> uint16
		 * chunk biome center -> vec
		 * requires a chunk struct
		 */
		const ushort version = 1;
		public uint Height;
		public uint Width;
		private ushort[] Chunks;
		private Chunk[] RawChunks;

		public GlobalData(uint w, uint h)
		{
			Height = h;
			Width = w;
			RawChunks = new Chunk[h * w];
			Chunks = new ushort[h * w];
		}

		public void SetChunk(uint x, uint y, Chunk c)
		{
			RawChunks[x + y * Width] = c;
		}

		public Chunk GetChunk(uint x, uint y)
		{
			return RawChunks[x + y * Width];
		}

		/* [0000 0000 0000 0000]
		 * [iiii xxxxxx yyyyyy]
		 * 
		 */

		private void ToBinaryArray()
		{
			for (int i = 0; i < RawChunks.Length; i++)
			{
				//first store info into integers
				var cnk = RawChunks[i];
				var id = (uint)cnk.BiomeID;
				var cx = (uint)cnk.BiomeCenter[0];
				var cy = (uint)cnk.BiomeCenter[1];

				//second, shift everything into the corect place
				id = id << 12;
				cx = cx << 6;

				id = (ushort)id;
				cx = (ushort)cx;
				cy = (ushort)cy;

				//third, bitwise AND everything with corect thing in order to set everything up for adition
				id = id & 0b1111_000000_000000;
				cx = cx & 0b0000_111111_000000;
				cy = cy & 0b0000_000000_111111;

				//forth, add everything together, and cast to a short
				var total = (ushort)(id | cy | cx);

				//finaly, set the binary array to the total
				Chunks[i] = total;
			}
		}

		private void ToChunkArray()
		{
			RawChunks = new Chunk[Chunks.Length];
			for (uint i = 0; i < Chunks.Length; i++)
			{
				/* Chunk Data Layout
				 * 0biiii_xxxxxx_yyyyyy
				 */

				//set up temperary holders
				var cdata = Chunks[i];
				var bd = cdata & 0b1111_000000_000000;
				var cx = cdata & 0b0000_111111_000000;
				var cy = cdata & 0b0000_000000_111111;

				//shift everything to proper location
				bd = bd >> 12;
				cx = cx >> 6;

				bd = (ushort)bd;
				cx = (ushort)cx;
				cy = (ushort)cy;

				Chunk rc = new Chunk((byte)bd, new Vec() { cx, cy });

				// set everything to proper thing
				RawChunks[i] = rc;
			}
		}

		public void Save(string filename)
		{
			using(BinaryWriter bw = new BinaryWriter(File.Open(filename, FileMode.Create)))
			{
				ToBinaryArray();
				bw.Write(version);
				bw.Write(Width);
				bw.Write(Height);
				UInt32 len = (UInt32)Chunks.Length;
				bw.Write(Chunks.Length);
				for(int i = 0; i < len; i++)
				{
					bw.Write(Chunks[i]);
				}
			}
		}

		public GlobalData Load(string filename)
		{
			using(BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open)))
			{
				var ver = br.ReadUInt16();
				Width = br.ReadUInt32();
				Height = br.ReadUInt32();
				var len = br.ReadUInt32();
				var temp = new ushort[len];
				for (int i = 0; i < len; i++)
				{
					temp[i] = br.ReadUInt16();
				}
				Chunks = temp;
				ToChunkArray();
			}
			return this;
		}

		public GlobalData(string filename)
		{
			using (BinaryReader br = new BinaryReader(File.Open(filename, FileMode.Open)))
			{
				var ver = br.ReadUInt16();
				Width = br.ReadUInt32();
				Height = br.ReadUInt32();
				var len = br.ReadUInt32();
				var temp = new ushort[len];
				for (int i = 0; i < len; i++)
				{
					temp[i] = br.ReadUInt16();
				}
				Chunks = temp;

				RawChunks = new Chunk[Chunks.Length];
				for (uint i = 0; i < Chunks.Length; i++)
				{
					/* Chunk Data Layout
					 * 0biiii_xxxxxx_yyyyyy
					 */

					//set up temperary holders
					var cdata = Chunks[i];
					var bd = cdata & 0b1111_000000_000000;
					var cx = cdata & 0b0000_111111_000000;
					var cy = cdata & 0b0000_000000_111111;

					//shift everything to proper location
					bd = bd >> 12;
					cx = cx >> 6;

					bd = (ushort)bd;
					cx = (ushort)cx;
					cy = (ushort)cy;

					Chunk rc = new Chunk((byte)bd, new Vec() { cx, cy });

					// set everything to proper thing
					RawChunks[i] = rc;
				}
			}
		}
	}	
}
