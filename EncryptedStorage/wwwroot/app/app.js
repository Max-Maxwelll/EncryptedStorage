var project = angular.module('EncryptedStorage', ['ngRoute']);

// project.value('AUTH', false);
// project.value('USER', new UserModel());
project.constant('SERVER_HOST', '/');

project.run(function($rootScope, $location, $http, $templateCache, UserService, ManageService, StorageService, AccountService, FileService) {
    
    if($location.path() != '/signin' && $location.path() != '/signup'){
        ManageService.GetUser();
    }

    $rootScope.$on('$routeChangeStart', function(event, next, current) {
        if($location.path() != '/signin' && $location.path() != '/signup'){
            ManageService.GetUser();
        }
    });
    $rootScope.$on('$viewContentLoaded', function() {
        $templateCache.removeAll();
    });
    $rootScope.$watch('selectedStorage', function (newValue, oldValue) { 
        StorageService.GetStorages();
        AccountService.GetAccounts();
        FileService.GetFiles();
    });
});