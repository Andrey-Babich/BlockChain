using BlockChain.Services;

var blockChainService = new BlockChainService();
var blockChainDisplayService = new BlockChainDisplayService();

blockChainService.AddBlock("Alice send Bob 100 Coin");
blockChainService.AddBlock("Bob send Marta 50 Coin");

blockChainDisplayService.ShowBlockChain(blockChainService.Chain);
blockChainDisplayService.ShowValidationResult(blockChainService.isValid());