project.service('UserService', function($http, $rootScope, SERVER_HOST){
    
    this.Register = function(model)
    {
        return $http.post(SERVER_HOST + "User/Register", model).then(
            function success (response) {
                $rootScope.response = response;
            },
            function error(response) {
                $rootScope.response = response;
            }
        );
    }

    this.Login = function(model)
    {
        return $http.post(SERVER_HOST+ "User/Login", model);
    }

    this.Test = function()
    {
        return $http.get(SERVER_HOST + 'User/Test');
    };

    this.Logout = function()
    {
        return $http.get(SERVER_HOST + 'User/Logout');
    };

    this.Confirm = function(model)
    {
        return $http.post(SERVER_HOST + 'User/ConfirmAction', model);
    };
});

project.service('ManageService', function($http, $rootScope, $location, SERVER_HOST, StorageService, AccountService, FileService){
    
    this.GetUser = function()
    {
        return $http.get(SERVER_HOST + "Manage/GetUser").then(
            function success (response) {        
                $rootScope.USER = response.data;
                $rootScope.AUTH = true;
                StorageService.GetStorages();
                StorageService.GetCurrent();
                AccountService.GetAccounts();
                FileService.GetFiles();
            },
            function error(response){
                $rootScope.AUTH = false;
                if($location.path() != '/signin' && $location.path() != '/signup')
                    $location.path("signin");
            }
        );
    }
});

project.service('StorageService', function($http, $rootScope, SERVER_HOST){
    this.InitializationTables = function()
    {
        return $http.get(SERVER_HOST + "Storage/InitializationTables");
    };

    this.GetStorages = function(func)
    {
        return $http.get(SERVER_HOST + "Storage/GetStorages").then(
            function success(response)
            {
                if(func != undefined)
                    $rootScope.$eval(func(response));
                $rootScope.storages = response.data;
            },
            function error(response)
            {
                if(func != undefined)
                    $rootScope.$eval(func(response));
                $rootScope.storages = [];
            }
        );
    };

    this.GetCurrent = function()
    {
        return $http.get(SERVER_HOST + "Storage/GetCurrent").then(
            function success(response){
                $rootScope.selectedStorage = response.data;
            },
            function error(response){
                console.log(response.data);
            }
        );
    };

    this.CreateStorage = function(model)
    {
        return $http.post(SERVER_HOST + "Storage/CreateStorage", model);
    };

    this.ChangeKeyStorage = function(model, func)
    {
        $http.post(SERVER_HOST + "Storage/ChangeKeyStorage", model).then(
            function success(response)
            {
                if(func != undefined)
                    $rootScope.$eval(func(response));
            },
            function error(response)
            {
                if(func != undefined)
                    $rootScope.$eval(func(response));
            }
        );
    };

    this.Delete = function(name)
    {
        return $http.get(SERVER_HOST + "Storage/Delete/" + name);
    };

    this.DeleteAll = function()
    {
        return $http.get(SERVER_HOST + "Storage/DeleteAll");
    };
    
    this.EnterKey = function(storage, model, func)
    {
        $http.post(SERVER_HOST + "Storage/EnterKey", model).then(
            function success(response){
                $rootScope.selectedStorage = storage;
                if(func != undefined)
                    $rootScope.$eval(func(response));
            },
            function error(response){
                if(func != undefined)
                    $rootScope.$eval(func(response));
            }
        );
    };

    this.UploadKey = function(files)
    {
        if (files.length === 0)
            return;

        const formData = new FormData();

        for (var i = 0; i < files.length ; i++) {
            formData.append(files[i].name, files[i]);
        }

        $http.post(SERVER_HOST + "Storage/UploadKey", formData, {
            headers: {'Content-Type': undefined}
        }).then(
            function success (response) {
                $rootScope.response = response;
            },
            function error (response) {
                $rootScope.response = response;
            }
        );
    };
});

project.service('AccountService', function($http, $rootScope, SERVER_HOST){
    this.GetAccounts = function(func)
    {
        return $http.get(SERVER_HOST + "Account/GetAccounts").then(
            function success(response)
            {
                // console.log(response.data);
                if(func != undefined)
                    $rootScope.$eval(func(response));
                $rootScope.accounts = response.data;
            },
            function error(response)
            {
                // console.log(response.data);
                if(func != undefined)
                    $rootScope.$eval(func(response));
                $rootScope.accounts = [];
            }
        );;
    };

    this.Create = function(model)
    {
        return $http.post(SERVER_HOST + "Account/Create", model);
    };

    this.Delete = function(id)
    {
        return $http.get(SERVER_HOST + "Account/Delete/" + id);
    };

    this.DeleteAll = function(func)
    {
        $http.get(SERVER_HOST + "Account/DeleteAll").then(
            function success(response){
                if(func != undefined)
                    $rootScope.$eval(func(response));
            },
            function error(response){
                if(func != undefined)
                    $rootScope.$eval(func(response));
            }
        );
    };

    this.Change = function(model)
    {
        return $http.post(SERVER_HOST + "Account/Change", model);
    };

    this.GetPassword = function(id)
    {
        return $http.get(SERVER_HOST + "Account/GetPassword/" + id).then(
            function success(response){
                navigator.clipboard.writeText(response.data)
                    .then(() => {
                        console.log("Скопировано");
                    })
                    .catch(err => {
                        console.log('Something went wrong', err);
                    });
            },
            function error(response){
                console.log(response.data);
            }
        );
    };
});

project.service('FileService', function($http, $rootScope, $location, $route, SERVER_HOST){
    this.GetFiles = function(func)
    {
        $http.get(SERVER_HOST + "File/GetFiles").then(
            function success(response) {
                $rootScope.files = response.data;
            },
            function error(response) {
                console.log(response.data);
            }
        );
    };

    this.Upload = function(name, files)
    {
        if (files.length === 0)
            return;

        const formData = new FormData();

        for (var i = 0; i < files.length ; i++) {
            formData.append(name, files[i]);
        }

        $http.post(SERVER_HOST + "File/Upload", formData, {
            headers: {'Content-Type': undefined}
        }).then(
            function success (response) {
                $rootScope.response = response;
                $route.reload();
            },
            function error (response) {
                $rootScope.response = response;
            }
        );
    };

    this.Delete = function(name)
    {
        $http.get(SERVER_HOST + "File/Delete/" + name).then(
            function success(response){
                $rootScope.response = response;
                $route.reload();
            },
            function error(response){
                $rootScope.response = response;
            }
        );
    };

    this.GetEncryptFile = function(name, link)
    {
        $http.get("/File/GetEncryptFile/" + name, { responseType: 'arraybuffer' }).then(
            function success(response){
                var contentDisposition = response.headers('content-disposition').split(';');
                var nameFile = contentDisposition[1].split('=')[1];
                var type = response.headers('content-type');
                var blob = new Blob([response.data], { type: type, responseType: 'arraybuffer' });
                var url = URL.createObjectURL(blob);
                
                link.setAttribute('href', url);
                link.setAttribute("download", nameFile);
                link.click();
            },
            function error(response){
                $rootScope.response = response;
            }
        );
    };

    this.GetDecryptFile = function(name, link)
    {
        $http.get("/File/GetDecryptFile/" + name, { responseType: 'arraybuffer' }).then(
            function success(response){
                var contentDisposition = response.headers('content-disposition').split(';');
                var nameFile = contentDisposition[1].split('=')[1];
                var type = response.headers('content-type');
                var blob = new Blob([response.data], { type: type, responseType: 'arraybuffer' });
                var url = URL.createObjectURL(blob);
                console.log(type);
                link.setAttribute('href', url);
                link.setAttribute("download", nameFile);
                link.click();
            },
            function error(response){
                $rootScope.response = response;
            }
        );
    };
});