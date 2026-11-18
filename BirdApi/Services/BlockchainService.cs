using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using Nethereum.Util;

namespace BirdApi.Services;
//---рекомендую паралельно открыть файл AiBirdsSol/src/BirdPass.sol
public class BlockchainService
{
    private readonly Web3 _web3;//---------------------------для библиотеки веб 3 которая общается с блокчейном
    private readonly string _contractAddress;//--------------для обьявления контракта в блокчейне
    private readonly string _rpcUrl;//-----------------------порт для общения с блокчейном
    public BlockchainService(IConfiguration configuration)//-IConfiguration дотсает информацию из appsettings json
    {
        _rpcUrl = configuration["Blockchain:RpcUrl"];//----записывает информацию порта из json либо если нет то конкретный порт
        _contractAddress = configuration["Blockchain:ContractAddress"];//-------берет адресс контракта из json
        _web3 = new Web3(_rpcUrl);//--------------------------------------------------записывает порт в обьект библиотеки который общается с блокчейном
    }
    //---функция для проверки подписки по 
    //---walletAddress адресу кошелька
    public async Task<bool> HasSubscriptionAsync(string walletAddress)
    {
        try
        {
            //---создание типо контракта для общения с блокчейном, 
            //---_web3(обьект с библиотекой для общения с блокчейном)
            //---.Eth - для передачи эфира\этериума
            //---.GetContract - получение контракта
            //---AbiJson - инструкция для работы с блокчейном
            //---_contractAddress - адрес на котором находиться контракт в блокчейне
            var contract = _web3.Eth.GetContract(AbiJson, _contractAddress);
            //---по адресу контракту берем контретную функцию hasSubscription
            var function = contract.GetFunction("IsSubscription");
            //---вызоф функции 
            //---CallAsync<bool> - асинхронный вызов, bool - то какой должен быть ответ от блокчейна
            //---передаем адресс кошелька walletAddress чтобы понять наличие подписки у пользователя с таким кошельком
            return await function.CallAsync<bool>(walletAddress);
        }
        catch
        {
            return false;
        }
    }
    //---возврат окончания подписки по 
    //---walletAddress адресу кошелька
    public async Task<DateTime?> GetSubscriptionEndDateAsync(string walletAddress)
    {
        try
        {
            //---создание двойника контракта 
            //---web3 - для работой с блокчейном
            //---Eth  - потому что в моем блокчейне валюта єто єфир
            //---GetContract - создание копии контракта для общения с ним
            //---AbiJson, _contractAddress - аби это инструкция для общения с блокчейном и адресс контракта
            var contract = _web3.Eth.GetContract(AbiJson, _contractAddress);
            //---передаю функцию контракта по названию
            var function = contract.GetFunction("getSubscriptionEnd");
            //---unixTime - ответ от блокчейна, тут будет дата окончания подписки
            //---function - передаем функцию контракта блокчейна которому я передам адрес кошелька
            //---System.Numerics.BigInteger - ии посоветовала использовать тип данних бигинт потому что блокчейн можеь вернуть длинний ответ потому что оно будет в битном формате
            var unixTime = await function.CallAsync<System.Numerics.BigInteger>(walletAddress);
            //---проверка наличия ответа от блокчейна
            if (unixTime == 0) return null;
            //---преобразование битного ответа от солидити в настоящее время и возврат от функции
            return DateTimeOffset.FromUnixTimeSeconds((long)unixTime).UtcDateTime;
        }
        catch
        {
            return null;
        }
    }
    //---инструкция для работы с блокчейном
    //---для функций hasSubscription и getSubscriptionEnd
    private const string AbiJson = @"[
        {
            ""inputs"": [{""internalType"": ""address"", ""name"": ""user"", ""type"": ""address""}],
            ""name"": ""IsSubscription"",
            ""outputs"": [{""internalType"": ""bool"", ""name"": """", ""type"": ""bool""}],
            ""stateMutability"": ""view"",
            ""type"": ""function""
        },
        {
            ""inputs"": [{""internalType"": ""address"", ""name"": ""user"", ""type"": ""address""}],
            ""name"": ""getSubscriptionEnd"",
            ""outputs"": [{""internalType"": ""uint256"", ""name"": """", ""type"": ""uint256""}],
            ""stateMutability"": ""view"",
            ""type"": ""function""
        }
    ]";
}