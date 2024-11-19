
using System.Collections.Generic;
using System.Linq;
using ProjectBBF.Singleton;


public partial class DataManager : MonoBehaviourSingleton<DataManager>
{
    private void PartialBind()
    {
        // Resolver는 여기서 등록하기
        
        this
            .Bind<IItemDataResolver, ItemDataAddressableResolver>()
            ;
    }
}