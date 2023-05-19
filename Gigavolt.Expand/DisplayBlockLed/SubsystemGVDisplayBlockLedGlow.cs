using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game
{
	public class SubsystemGVDisplayBlockLedGlow : Subsystem, IDrawable
	{
		public SubsystemSky m_subsystemSky;
		public SubsystemTerrain m_subsystemTerrain;
		public readonly DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();
		public readonly Dictionary<GVDisplayBlockPoint, bool> m_points = new Dictionary<GVDisplayBlockPoint, bool>();

		public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

		public static int[] m_drawOrders = { 110 };

		public int[] DrawOrders => m_drawOrders;

		public GVDisplayBlockPoint AddGlowPoint()
		{
			var glowPoint = new GVDisplayBlockPoint();
			m_points.Add(glowPoint, value: true);
			return glowPoint;
		}

		public void RemoveGlowPoint(GVDisplayBlockPoint glowPoint)
		{
			m_points.Remove(glowPoint);
		}

		public void Draw(Camera camera, int drawOrder)
		{
			m_drawBlockEnvironmentData.SubsystemTerrain = m_subsystemTerrain;
			m_drawBlockEnvironmentData.InWorldMatrix = Matrix.Identity;
			foreach (GVDisplayBlockPoint key in m_points.Keys)
			{
				if(key.Value==0) continue;
				Vector3 position = key.Position;
				if (camera.ViewFrustum.Intersection(position))
				{
					int x = Terrain.ToCell(position.X);
					int num2 = Terrain.ToCell(position.Y);
					int z = Terrain.ToCell(position.Z);
					int num3 = Terrain.ExtractContents(key.Value);
					Block block = BlocksManager.Blocks[num3];
					TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(x, z);
					if (chunkAtCell != null && chunkAtCell.State >= TerrainChunkState.InvalidVertices1 && num2 >= 0 && num2 < 255)
					{
						m_drawBlockEnvironmentData.Humidity = m_subsystemTerrain.Terrain.GetSeasonalHumidity(x, z);
						m_drawBlockEnvironmentData.Temperature = m_subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(num2);
					}
					Matrix matrix = Matrix.CreateFromYawPitchRoll(key.Rotation.X, key.Rotation.Y, key.Rotation.Z);
					matrix.Translation = position;
					float size;
					if (key.Type == 1)
					{
						m_drawBlockEnvironmentData.Light = key.Light;
						size = key.Size;
					}
					else
					{
						m_drawBlockEnvironmentData.Light = m_subsystemTerrain.Terrain.GetCellLightFast(x, num2, z);
						size = block.InHandScale;
					}
					m_drawBlockEnvironmentData.BillboardDirection = (block.GetAlignToVelocity(key.Value) ? null : new Vector3?(camera.ViewDirection));
					m_drawBlockEnvironmentData.InWorldMatrix.Translation = position;
					block.DrawBlock(m_primitivesRenderer, key.Value, key.Color, size, ref matrix, m_drawBlockEnvironmentData);
				}
			}

			m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
		}

		public override void Load(ValuesDictionary valuesDictionary)
		{
			m_subsystemSky = Project.FindSubsystem<SubsystemSky>(throwOnError: true);
			m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(throwOnError: true);
		}
	}
}
