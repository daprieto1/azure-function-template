# Function Template

Este repositorio contiene los archivos necesarios para desplegar una 'Azure Function' haciendo uso de la definición por medio de templates y a través de una petición REST a un servicio del RunCloud.

## Definición del Template

`azuredeploy.json`

La siguientes son variables importantes a definir para realizar el despliegue:
* repoUrl: URL del repositorio donde se encuentra el código de la 'Function' a desplegar.
* branch: La rama del repositorio que se debe tomar al moemnto de hacer el despliegue.
* functionName: Nombre que se le asigna a la 'Function'.
* hostingPlanName: Representa la colección de recursos físicos para desplegar las aplicaciones.
* storageAccountName: Nombre de la cuenta de almacenamiento que se desea usar.

## Definición de los Parámetros

`azuredeploy.parameters.json`

* functionAppName: Nombre del 'App Service' a crear en Azure, el cuál puede agrupar multiples funciones.

## Application Settings

Administrar variables de entorno, versiones de los Frameworks, debug remoto, configuraciones de aplicación, cadenas de conexión, etc. Esas configuraciones son especificas para cada 'Function App'.

Para hacerlo configure la pareja llave valor en la sección del recurso de 'Function App'.

```
appSettings = [
		{
			"name": "DATABASE_CONNECTION",
			"value": "[parameters('databaseConnection')]"
		}
	]
```

## Despliegue de la Function

Realizar una petición `POST` a la siguiente URL usando en el body el objeto que aparece más abajo.

`http://{host}/BizagiCloudRun/api/DeployTemplate`

```
{
	"ResourceGroupName":"DIEGO-GR-EXP",
	"SubscriptionId":"eaf8a6a8-0539-41c8-9204-f1c25391ef08",
	"DeploymentName":"diegotest",
	"Template":"{\"$schema\":\"https://schema.management.azure.com/schemas/2015-01-01/deploymentTemplate.json#\",\"contentVersion\":\"1.0.0.0\",\"parameters\":{\"functionAppName\":{\"type\":\"string\",\"metadata\":{\"description\":\"The name of the function app to create. Must be globally unique.\"}}},\"variables\":{\"repoUrl\":\"https://github.com/daprieto1/azure-function-template.git\",\"branch\":\"master\",\"functionName\":\"Hello\",\"hostingPlanName\":\"[parameters('functionAppName')]\",\"storageAccountName\":\"diegostorageexperiments\",\"storageACcountid\":\"[resourceId('Microsoft.Storage/storageAccounts', variables('storageAccountName'))]\",\"LogicAppLocation\":\"[resourceGroup().location]\",\"storageAccountType\":\"Standard_LRS\"},\"resources\":[{\"apiVersion\":\"2015-08-01\",\"type\":\"Microsoft.Web/sites\",\"name\":\"[parameters('functionAppName')]\",\"location\":\"[resourceGroup().location]\",\"kind\":\"functionapp\",\"properties\":{\"siteConfig\":{\"appSettings\":[{\"name\":\"AzureWebJobsDashboard\",\"value\":\"[concat('DefaultEndpointsProtocol=https;AccountName=', variables('storageAccountName'), ';AccountKey=', listKeys(variables('storageAccountid'),'2015-05-01-preview').key1)]\"},{\"name\":\"AzureWebJobsStorage\",\"value\":\"[concat('DefaultEndpointsProtocol=https;AccountName=', variables('storageAccountName'), ';AccountKey=', listKeys(variables('storageAccountid'),'2015-05-01-preview').key1)]\"},{\"name\":\"WEBSITE_CONTENTAZUREFILECONNECTIONSTRING\",\"value\":\"[concat('DefaultEndpointsProtocol=https;AccountName=', variables('storageAccountName'), ';AccountKey=', listKeys(variables('storageAccountid'),'2015-05-01-preview').key1)]\"},{\"name\":\"WEBSITE_CONTENTSHARE\",\"value\":\"[toLower(parameters('functionAppName'))]\"},{\"name\":\"FUNCTIONS_EXTENSION_VERSION\",\"value\":\"~1\"},{\"name\":\"WEBSITE_NODE_DEFAULT_VERSION\",\"value\":\"6.5.0\"}]}},\"resources\":[{\"apiVersion\":\"2015-08-01\",\"name\":\"web\",\"type\":\"sourcecontrols\",\"dependsOn\":[\"[resourceId('Microsoft.Web/Sites', parameters('functionAppName'))]\"],\"properties\":{\"RepoUrl\":\"[variables('repoURL')]\",\"branch\":\"[variables('branch')]\",\"IsManualIntegration\":true}}]}]}",
	"Parameters":"{\"functionAppName\":{\"value\":\"RestBizagiTest4\"}}"
}
```

## Bibliografía
* https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-create-first-template
* https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-group-overview
* https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-export-template
* https://docs.microsoft.com/en-us/azure/app-service/azure-web-sites-web-hosting-plans-in-depth-overview
* https://docs.microsoft.com/en-us/azure/azure-functions/functions-how-to-use-azure-function-app-settings


