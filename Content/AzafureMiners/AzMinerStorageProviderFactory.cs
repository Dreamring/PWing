using System.Collections.Generic;
using InnoVault.Storages;
using Terraria;
using Terraria.DataStructures;

namespace PWing.Content.AzafureMiners
{
	internal class AzMinerStorageProviderFactory : IStorageProviderFactory
	{
		public string Identifier => "CE.AzMiner";

		public int Priority => 5;

		public bool IsAvailable => true;

		public IEnumerable<IStorageProvider> FindStorageProviders(Point16 position, int range, Item item)
		{
			AzMinerStorageProvider provider = AzMinerStorageProvider.FindNearPosition(position, range, item);
			if (provider != null)
			{
				yield return (IStorageProvider)(object)provider;
			}
		}

		public IStorageProvider GetStorageProviders(Point16 position, Item item)
		{
			return (IStorageProvider)(object)AzMinerStorageProvider.GetAtPosition(position, item);
		}
	}
}
