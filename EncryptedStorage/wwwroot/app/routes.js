angular.module('EncryptedStorage').config(function($routeProvider)
{
    $routeProvider
        .when('/', { templateUrl: '/app/views/storages.html', controller:'storagesCtrl'})
        .when('/signin', { templateUrl: '/app/views/signin.html', controller:'signinCtrl'})
        .when('/signup', { templateUrl: '/app/views/signup.html', controller:'signupCtrl'})
        .when('/files', { templateUrl: '/app/views/files.html', controller:'filesCtrl'})
        .when('/profile', { templateUrl: '/app/views/profile.html', controller:'profileCtrl'})
        // .when('/passports', { templateUrl: '/app/views/passports.html', controller:'passportsCtrl'})
        .when('/accounts', { templateUrl: '/app/views/accounts.html', controller:'accountsCtrl'})
        .when('/error', { templateUrl: '/app/views/error.html', controller:'errorCtrl'})
        .otherwise({redirectTo: '/error'});
});