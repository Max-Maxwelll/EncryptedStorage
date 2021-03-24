project.controller('accountsCtrl', function($scope, $rootScope, AccountService, StorageService){
    StorageService.GetCurrent();
    // $scope.CreateAccount = function()
    // {
    //     $scope.isshowCreate = true;
    // };

    $scope.DeleteAll = function()
    {
        $rootScope.response = AccountService.DeleteAll(function(response){
            $rootScope.response = response;
            AccountService.GetAccounts();
        });
    };
    
    $scope.LoadAccounts = function()
    {
        AccountService.GetAccounts();
    };
    $scope.Delete = function(id)
    {
        AccountService.Delete(id).then(
            function success(response){
                $rootScope.response = response;
            },
            function error(response){
                $rootScope.response = response;
            }
        );
        AccountService.GetAccounts();
    };
    $scope.CopyPassword = function(id)
    {
        AccountService.GetPassword(id);
    };
});

// project.controller('passportsCtrl', function($scope){

//     $scope.createPassport = function()
//     {
//         console.log("add")
//         $scope.isshowCreate = true;
//     };
//     $scope.changePassport = function()
//     {
//         console.log("change")
//         $scope.isshowChange = true;
//     };
// });

project.controller('storagesCtrl', function($scope, $rootScope, StorageService){

    // $scope.storage = new StorageModel();
    // $scope.checkedStorages = Array();
    // $scope.isConfirm = false;
    // $scope.SelectStorage = function(id)
    // {
    //     $scope.confirm();
    //     $scope.storage.id = id;
    // };
    $scope.DownloadStorage = function(){
        $scope.Confirm();
    }
    $scope.DeleteStorage = function(storage){
        // $scope.storage = storage;
        StorageService.Delete(storage.name).then(
            function success(response){
                $rootScope.response = response;
                StorageService.GetStorages();
            },
            function error(response){
                $rootScope.response = response;
            }
        );
    };

    $scope.DeleteAllStorage = function(){
        StorageService.DeleteAll().then(
            function success(response){
                StorageService.GetStorages();
            },
            function error(response){
                $rootScope.response = response;
            }
        );
    };

    $scope.CheckAll = function(){
        document.getElementsByName("checkStorages").forEach(function(checkbox){
            checkbox.checked = !checkbox.checked;
        });
    };

    $scope.Check = function($event){
        
        $scope.checkedStorages.push(event.srcElement);
        console.log($scope.checkedStorages)
    };

    // $scope.$watch("isshowConfirm", function (newValue, oldValue) 
    // {
    //     console.log("isConfirm changed");
    //     if($scope.isConfirm) {
    //         StorageService.Delete($scope.storage.name).then(
    //             function success(response){
    //                 console.log(response.data);
    //                 StorageService.GetStorages();
    //             },
    //             function error(response){
    //                 console.log(response.data);
    //             }
    //         );
    //         // $scope.isshowConfirm = false;
    //     }    
    //     else {
    //         // $scope.isshowConfirm = false;
    //     }           
    // });
});

project.controller('filesCtrl', function($scope, $rootScope, $http, $location, FileService){
    $scope.Delete = function(name)
    {
        FileService.Delete(name);
    };

    $scope.GetEncryptFile = function(name)
    {
        var element = document.getElementsByName('EncryptFile')[0];
        FileService.GetEncryptFile(name, element);
    };

    $scope.GetDecryptFile = function(name)
    {
        var element = document.getElementsByName('DecryptFile')[0];
        FileService.GetDecryptFile(name, element);
    };
});

project.controller('signinCtrl', function($scope, $rootScope, $location, UserService){

    $scope.loginModel = new LoginModel();

    $scope.submit = function(authPanel){
        if(authPanel.$valid){
            UserService.Login($scope.loginModel).then(
                function success (response) {
                    $rootScope.response = response;
                    $location.path("/");
                },
                function error(response) {
                    $rootScope.response = response;
                }
            );
        }
    }

    $scope.forgotPassword = function(){
        
    };
});

project.controller('signupCtrl', function($scope, $rootScope, UserService){

    $scope.registerModel = new RegisterModel();

    $scope.submit = function(authPanel){
        if(authPanel.$valid){
            UserService.Register($scope.registerModel);
        }
        else{
            $rootScope.response = {data: "Формат данных не верен", status: 400};
        }
    }
});