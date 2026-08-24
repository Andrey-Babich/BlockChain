using BlockChain.Services;

var blockChainService = new BlockChainService();
var blockChainDisplayService = new BlockChainDisplayService();

blockChainService.AddBlock("Бабіч");
blockChainService.AddBlock("Бабіч");

blockChainDisplayService.ShowBlockChain(blockChainService.Chain);
blockChainDisplayService.ShowValidationResult(blockChainService.isValid());