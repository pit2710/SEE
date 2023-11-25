# Einrichten des FullBodyPlayers in Virtual Reality in SEE:

1. Unity Editor öffnen. Unter `Edit -> Project Settings` zum Punkt `XR Plug-in Management` navigieren. 
Einen Haken setzen bei `Initialize XR on Startup` und `Open XR`.
2. Über `File -> Open Scene` die Scene `SEEStart` auswählen. Die Scenen sind unter `\Assets\Scenes` zu finden. Im `NetworkManager` in der `Network Prefabs Lists` die `SEENetworkPrefabs` 
öffnen und anstelle des ersten Avatars den `FullBodyPlayer.prefab` auswählen.
3. Wie bei Punkt zwei, die Scene `SEEWorld` öffnen. Unter PlayerSpawn anstelle des ersten Avatars den `FullBodyPlayer.prefab` setzen. Der FullBodyPlayer befindet sich im Verzeichnis `\Assets\Resources\Prefabs\Players`.
4. In der Scene SEEWorld sollte die Dicke der generierten Nodes etwas vergrößert werden, damit wir ungewollte Collision verhindern. Dafür wird im `ArchitectureTable -> ReflexionCity` das Script `SEE Reflexion City (Script)` 
im Inspektor ausgewählt. Den Tab `Nodes` anwählen, `NodeTypes` und `Map` ausklappen. Bei `Module` wird `Metric To length` ausgeklappt und der mittlere Wert von 0.001 auf 0.01 erhöht. Gleiches passiert unter `UNKNOWNTYPE`. 
Nun kann die City generiert werden.
5. Im Script `XRCameraRigManager.cs` (`\Assets\SEE\XR`) werden in Zeile 21 und 27 Namen für die VR-Controller vergeben. `"Camera Offset/LeftHand Controller"` muss zu `"TrackerOffsets/Controller (left)"` geändert werden. 
Gleiches gilt für den rechten Controller (`"TrackerOffsets/Controller (right)"`). 

