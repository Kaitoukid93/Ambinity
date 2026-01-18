
using System;
using System.Collections.Generic;
namespace AmbinityCore.Models.Profile;

public interface IEmbeddedAssetRepository<T>
{
    IReadOnlyList<T> LoadAll();
    //     T Load(string id);
}
