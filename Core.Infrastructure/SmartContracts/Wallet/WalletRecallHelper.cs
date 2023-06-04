using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Infrastructure.SmartContracts.Wallet
{
    public class WalletRecallHelper
    {
        public string PublishKey { get; set; }

        public static List<WalletRecallHelper> Wallets = new List<WalletRecallHelper>
        {
            //new WalletRecallHelper {
            //    PublishKey = "0xb12E5e1115cbe9b55bAC61CC9A28ccFF28E19d88" 
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x35662E82e769dF90AE6E4fAD81070888DB7e16d9"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x16D2F95F27147F8Cb18CdA28D609d273b118f144"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x42d93F852bfC2b40A7e4146Bf06e7C9e6A8b47A5"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x7C75583c555A1897e359C5fE045A33ca778dD7c5"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x59A890ac767f94A8704151869e8fB3d3aDE342A0"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x3AFA3Ea4d11c2C8a363bB8b81d171E5bA44dB49b"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x2D768727Ac9314cebED9E074d0902ce1d95aD974"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x752A3852e70Dd6ff79446808a93807fba25E0d92"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0xb1a70C249f0Da2f9a1FeC788638745131E07FB94"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x99c676B1D04e5c97eCb0E67c9983e3D8Ca92Fc51"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x2FbdE7B0104b29bbf6E1c8653C748DE508Ae0821"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x1B27f7b1b7fb6816aA8a50049465b4dF995fA6B9"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0xB7a35582E1c0F2fbddF8E3ED8A4e561CA21a638C"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x1b449FC208767D23B01fD8e99D76033456eb938b"
            //},
            // new WalletRecallHelper {
            //    PublishKey = "0xD50cD3CDbCAAEa57870fE98C508209a4E5e8c735"
            //},
            // new WalletRecallHelper {
            //    PublishKey = "0x6e7b86D7A04A1084FC39F1A55455f44881F06Efb"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0xFBefBFfB393E553bcf42b131573D271C756a0d1B"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x0D45410d518B3077cb2eEE719AC7a83bad5BBf76"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x4ddF28DD27DC8FE973C3d806B1E5f59997CEC1E2"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x49d1825ba2514B5cf325342fDd22885a45d2A87E"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0xFB3c4b4054a6CaE5b9dABA028EC3643FbAdB6EC7"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0x81ef4D723223e2619E1D27C43C654f0E52218A86"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0xce44E11274006AfB386F1b659C055ca8B9Eb49e2"
            //},
            //new WalletRecallHelper {
            //    PublishKey = "0xECf2C61BA88E7670B871a6fB4D70B818b7c07564"
            //},

            new WalletRecallHelper {
                PublishKey = "0x9e894dDef1ccB5cbA2b635E89555EA368cB7475E"
            },
            new WalletRecallHelper {
                PublishKey = "0x66806a8a72343D9B53913bcF14902242fCFE046a"
            },
            new WalletRecallHelper {
                PublishKey = "0xC6ce57e0f86973936844b017452A85c121D4ea37"
            },
            new WalletRecallHelper {
                PublishKey = "0xAaa6eBe3C07Eb66fB0E6d65B514f556b7776c05D"
            },
            new WalletRecallHelper {
                PublishKey = "0x090E41ee7593169Fb2F337686f0ad0781f1D3905"
            },
            new WalletRecallHelper {
                PublishKey = "0x74B77f7A6734639359d966ECf2954b32599Ec09d"
            },
            new WalletRecallHelper {
                PublishKey = "0xa81D9ea54390f164e219627975885AaEB9aF1429"
            },
            new WalletRecallHelper {
                PublishKey = "0xb3E122A085236BAC92334Ec33D53C18FdA9f7868"
            },
            new WalletRecallHelper {
                PublishKey = "0x6f6160FD6F3508b284e6cA27E5631D729f8A8D6E"
            },
            new WalletRecallHelper {
                PublishKey = "0xe22DD672eb67Db8347cf934c1B85f0A1cd0eBbeF"
            },
            new WalletRecallHelper {
                PublishKey = "0x945F760F9d96473660612FddEF06f1C38ecE5dD6"
            },
            new WalletRecallHelper {
                PublishKey = "0x4f4f24205b809b04a6f64D51EBa8d4739676B97D"
            },
            new WalletRecallHelper {
                PublishKey = "0x720b1D219a161635e4eA65C63Ee41491DC240139"
            },
            new WalletRecallHelper {
                PublishKey = "0x82AbbD173bC2D0CB463752fc98eA6559a45Bb1A8"
            },
            new WalletRecallHelper {
                PublishKey = "0x1145282CBed3E407259a7dD99aB100fDc245421A"
            },
             new WalletRecallHelper {
                PublishKey = "0xD61179020Ed2fd5db892B4FEa3E241e85E96fD15"
            },
             new WalletRecallHelper {
                PublishKey = "0xCc9Cb87e8DFd113fE927fA180DA6221546c1E7AD"
            },
            new WalletRecallHelper {
                PublishKey = "0x4E2e267D487a70e883f316B19D003077620B3E98"
            },
            new WalletRecallHelper {
                PublishKey = "0xe3D44B7923ff6a0b36c76a072e588aA596Fc19F2"
            },
            new WalletRecallHelper {
                PublishKey = "0xe3c2A734581F56C52C861499e6f5d01e5383611D"
            },
            new WalletRecallHelper {
                PublishKey = "0xFF49C0bcA68088cE6Dd56ef25f3C7d1AB44D0A93"
            },
            new WalletRecallHelper {
                PublishKey = "0xc129c765098CCc1bCBA91fFABbD31594f138Cc48"
            },
            new WalletRecallHelper {
                PublishKey = "0x8B392ECA57BDE6399cA23fEdc22E97B1ccC3ccF2"
            },
            new WalletRecallHelper {
                PublishKey = "0xe26Fc45D529AdE7a3664f0f1DC7F47C14902CD05"
            },
            new WalletRecallHelper {
                PublishKey = "0xb78028498321f7cF75B942dE0186cAd9dc00576c"
            },
            new WalletRecallHelper {
                PublishKey = "0x03567bfFF72CA03f01f1B54B45cF562129D1200c"
            },
        };


        public static WalletRecallHelper GetWalletRamdom()
        {
            Random random = new Random();

            var nextIndex = random.Next(0, Wallets.Count());

            var value = Wallets[nextIndex];

            return value;
        }
    }
}
