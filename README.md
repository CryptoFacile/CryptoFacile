# CryptoFacile
Logiciel très simple d'utilisation. Il permet de miner près de 14
cryptomonnaies différentes, dont Ethereum et Monero.

![Alt text](img/screen-2.png?raw=true "Crypto Facile")

![Alt text](img/screen-1.png?raw=true "Crypto Facile")

# Comment démarrer
-  Compiler avec Visual Studio 2019
-  Démarrer le fichier ainsi obtenue CryptoFacile.exe

# Configuration des pools
Les fichiers de configuration des pools ce trouve à la racine dans le dossier \poolconfig

### Exemple d'un fichier pool.conf
> [PoolConfig]
Name=Default
Type=GPU
Custom=-pool ethash.poolbinance.com:8888 -wal "NAME" -worker "NAME"-epsw x -asm 2 -dbg -1 -allpools 1 -mode 1 -log 0
