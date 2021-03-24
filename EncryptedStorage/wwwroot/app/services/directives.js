
project.directive('sidebar', function($http, $rootScope, $timeout, $templateCache, $route, $location, UserService, StorageService)
{
    return {
        link: function(scope){
            // $rootScope.$watch("storages", function (newValue, oldValue) 
            // {
            //     if($rootScope.storages != undefined && $rootScope.storages != null && $rootScope.storages.length != 0){
            //         scope.selectedStorage = $rootScope.storages[0].id;
            //         scope.isshowKey = true;
            //     }                    
            // });
            
            scope.SelectStorage = function(id){
                scope.selectedStorage = id;
                scope.EnterKey();
            };

            scope.EnterKey = function(){
                scope.isshowKey = true;
            };

            // scope.ChangePassword = function(){
            //     scope.isshowChangePassword = true;
            // };

            // scope.ChangeEmail = function(){
            //     scope.isshowChangeEmail = true;
            // };

            scope.Logout = function(){
                UserService.Logout().then(
                    function success(){
                        $timeout(function () {
                            // 0 ms delay to reload the page.
                            $route.reload();
                        }, 0);
                    }
                );
            };

            $rootScope.$watch("AUTH",function(newValue,oldValue) {
                $templateCache.removeAll();
            });
            // scope.config = config;

            // scope.$watch('profile', function (newValue, oldValue) { 
            //         if (newValue == oldValue ) {
            //             return;
            //         }
            //         // scope. = scope.$root.user;

            //         console.log('sidebar');
            //         // console.log(newValue)
            //         // console.log(oldValue)
            //     }
            // );

            
        },
        restrict: 'A',
        templateUrl: 'app/views/directives/sidebar.html'
    };
});

project.directive('select', function($window) {
    function main(scope, element, attrs) {
        var options = angular.element(element.find('div')[2]);
        var arrow = angular.element(angular.element(angular.element(element.find('div')[0]).find('div')[0]).find('span')[1]);
        var hint = angular.element(element.find('div')[3]);
        scope.selected = scope.options[0];

        if(scope.colors != null){
            scope.options = scope.colors;
            scope.selected = scope.colors[1];
        }

        scope.optionClick = function(selected){
            scope.selected = selected;
            options.css('display', 'none');
            options.css('height', 0);
        }
        scope.showOptions = function(){
            // options.css('width', document.querySelector('.select').clientWidth + 2 + "px");
            if(options.css('height') == '0px' || options.css('height') == '' || options.css('height') == 0){
                arrow.removeClass('select-arrow-off');
                arrow.addClass('select-arrow-on');
                options.css('display', 'block');
                options.css('height', '300px');
                // elem.css('animation', 'onOptions 0.3s ease-out')
            }else{
                arrow.addClass('select-arrow-off');
                arrow.removeClass('select-arrow-on');
                options.css('display', 'none');
                options.css('height', 0);
                // elem.css('animation', '');
            }
            
        }
        scope.blur = function(){
            hint.css('top', '-70px');
            hint.css('opacity', 0);
            setTimeout(function(){
                arrow.addClass('select-arrow-off');
                arrow.removeClass('select-arrow-on');
                options.css('display', 'none');
                options.css('height', 0);
            },500)
            
        }
        scope.mouseenter = function(){
            hint.css('top', '-40px');
            hint.css('opacity', 0.8);
        }
    }

    return {
        link: main,
        scope: {
            title: '@title',
            options: '=select',
            selected: '=selected',
            colors: '=colors'
        },
        templateUrl: 'views/directives/select.html',

    };
});

project.directive('inputNumber', function($window) {
    function main(scope, element, attrs) {
        var number = scope.number;
        var hint = angular.element(element.find('div')[1]);

        scope.change = function(){
            var pattern = /^[0-9]*$/;  
            
            if(pattern.test(scope.number)){
                number = scope.number;
            }
            else{
                scope.number = number;
            }
               
        };

        scope.mouseenter = function(){
            hint.css('top', '-40px');
            hint.css('opacity', 0.8);
        }
        scope.blur = function(){
            hint.css('top', '-70px');
            hint.css('opacity', 0);
        }
    }

    return {
        restrict: 'E',
        link: main,
        scope: {
            title : '@title',
            number : '=number',
        },
        templateUrl: 'views/directives/input-number.html',

    };
});

project.directive('modalMessage', function($window, $rootScope) {
    function main(scope, element, attrs) {
        scope.isshow = false;
        scope.title = "Сообщение";
        scope.oldResponse = $rootScope.response;

        scope.Close = function()
        {
            scope.isshow = !scope.isshow;
        }

        $rootScope.$watch("response", function (newValue, oldValue) 
        {
            if($rootScope.response == null)
                scope.isshow = false;
            if($rootScope.response == scope.oldResponse)
                return;
            if($rootScope.response.status == 200) scope.title = "Сообщение";
            else scope.title = "Ошибка"; 
            scope.isshow = true;              
        });
    }

    return {
        link: main,
        restrict: "E",
        scope: {
            response: '=response'
        },
        templateUrl: '/app/views/directives/modal-message.html',

    };
});

project.directive('ngChangeAccount', function($window, AccountService) {
    function main(scope, element, attrs) {
        scope.isshow = false;
        // scope.account = new AccountModel();
        scope.Show = function()
        {
            scope.isshow = !scope.isshow;
        };
        
        scope.Change = function(model)
        {
            AccountService.Change(model).then(
                function success(response){
                    console.log(response.data);
                    AccountService.GetAccounts();
                },
                function error(response){
                    console.log(response.data);
                }
            );
        };

        scope.Delete = function(id)
        {
            AccountService.Delete(id).then(
                function success(response){
                    console.log(response.data);
                    AccountService.GetAccounts();
                },
                function error(response){
                    console.log(response.data);
                }
            );
        };
    }

    return {
        link: main,
        restrict: "A",
        scope: {
            account: '=ngChangeAccount'
        },
        transclude: true,
        templateUrl: '/app/views/directives/modal-change-account.html',

    };
});

project.directive('ngCreateAccount', function($window, $rootScope, AccountService) {
    function main(scope, element, attrs) {
        scope.isshow = false;
        scope.account = new AccountModel();
        scope.Open = function()
        {
            $rootScope.response = null;
            scope.isshow = !scope.isshow;
        };
        scope.Submit = function()
        {
            AccountService.Create(scope.account).then(
                function success(response){
                    AccountService.GetAccounts();
                    $rootScope.response = response;
                    scope.isshow = false;
                },
                function error(response){
                    $rootScope.response = response;
                    scope.isshow = false;
                }
            );
        };
    }

    return {
        link: main,
        restrict: "A",
        scope: {
            // account: '=ngCreateAccount'
        },
        transclude: true,
        templateUrl: '/app/views/directives/modal-create-account.html',

    };
});

// project.directive('modalChangePassport', function($window) {
//     function main(scope, element, attrs) {
//         scope.isshow = false;
//         scope.passport = new Passport();
//         scope.close = function()
//         {
//             scope.isshow = !scope.isshow;
//         }
//     }

//     return {
//         link: main,
//         restrict: "E",
//         scope: {
//             // title: '@title',
//             title: '@title',
//             message: '@message',
//             passport: '=passport',
//             isshow: '=isshow'
//         },
//         templateUrl: '/app/views/directives/modal-change-passport.html',

//     };
// });

// project.directive('modalCreatePassport', function($window) {
//     function main(scope, element, attrs) {
//         scope.isshow = false;
//         scope.passport = new Passport();
//         scope.close = function()
//         {
//             scope.isshow = !scope.isshow;
//         }
//     }

//     return {
//         link: main,
//         restrict: "E",
//         scope: {
//             // title: '@title',
//             title: '@title',
//             message: '@message',
//             passport: '=passport',
//             isshow: '=isshow'
//         },
//         templateUrl: '/app/views/directives/modal-create-passport.html',

//     };
// });

project.directive('ngCreateStorage', function($window, $rootScope, StorageService) {
    function main(scope, element, attrs) {
        scope.isshow = false;
        scope.storageModel = new StorageRequestModel();
        scope.Show = function()
        {
            scope.isshow = !scope.isshow;
        }

        scope.Submit = function()
        {
            console.log(scope.storageModel)
            StorageService.CreateStorage(scope.storageModel).then(
                function success(response)
                {
                    StorageService.GetStorages();
                    $rootScope.response = response;
                    scope.isshow = false;
                },
                function error(response)
                {
                    $rootScope.response = response;
                }
            );
        };
    }

    return {
        link: main,
        restrict: "A",
        transclude: true,
        templateUrl: '/app/views/directives/modal-create-storage.html',

    };
});

project.directive('ngChangeKey', function($window, $rootScope, StorageService) {
    function main(scope, element, attrs) {
        scope.isshow = false;
        scope.changeKeyModel = new ChangeKeyStorageModel();
        scope.Show = function()
        {
            scope.isshow = !scope.isshow;
        }

        scope.Submit = function()
        {
            if(scope.storage.name == null || scope.storage.name == ""){
                console.log("Имя отсутствует");
                return;
            }
            scope.changeKeyModel.Name = scope.storage.name

            StorageService.ChangeKeyStorage(scope.changeKeyModel, function(response){
                $rootScope.response = response;
                scope.isshow = false;
            });
        };
    }

    return {
        link: main,
        restrict: "A",
        scope: {
            storage: '=ngChangeKey'
        },
        transclude: true,
        templateUrl: '/app/views/directives/modal-change-key.html',

    };
});

project.directive('ngConfirm', function($window, $rootScope, UserService) {
    function main(scope, element, attrs) {
        scope.confirmModel = new ConfirmModel();
        scope.isshow = false;
        scope.Open = function(event) {
            $rootScope.response = null;
            scope.isshow = !scope.isshow;
        };

        scope.Submit = function()
        {
            UserService.Confirm(scope.confirmModel).then(
                function success(response)
                {
                    scope.$eval(scope.ngFunction);
                    $rootScope.response = response;
                    scope.isshow = false;
                },
                function error(response)
                {
                    $rootScope.response = response;
                    scope.isshow = false;
                }
            );
        };
    };

    return {
        link: main,
        restrict: "A",
        scope: {
            message: '@ngConfirm',
            ngFunction: '&ngFunction'
        },
        transclude: true,
        templateUrl: '/app/views/directives/modal-confirm.html'
    };
});

project.directive('ngKey', function($window, $rootScope, StorageService, AccountService, FileService) {
    function main(scope, element, attrs) {
        scope.isshow = false;
        scope.keyModel = new KeyModel();

        if($rootScope.selectedStorage != undefined){
            if(scope.storage.name == $rootScope.selectedStorage.name)
                element.css("border-bottom", "1px solid #2196F3");
            else
                element.css("border-bottom", "1px solid #353535");
        }

        $rootScope.$watch('selectedStorage', function (newValue, oldValue) { 
            if($rootScope.selectedStorage != undefined){
                if(scope.storage.name == $rootScope.selectedStorage.name)
                    element.css("border-bottom", "1px solid #2196F3");
                else
                    element.css("border-bottom", "1px solid #353535");
            }
        });
        

        // $rootScope.$watch('file', function (newValue, oldValue) { 
            
        // });
        scope.Upload = function(files)
        {
            console.log("upload");
            StorageService.UploadKey(files);
            
        };
        scope.Submit = function()
        {
            scope.keyModel.StorageName = scope.storage.name;
            StorageService.EnterKey(scope.storage, scope.keyModel, function(response){
                $rootScope.response = response;
            });
            scope.isshow = false;
        };

        scope.Show = function()
        {
            $rootScope.response = null;
            scope.isshow = !scope.isshow;
        }
    }

    return {
        link: main,
        restrict: "A",
        scope: {
            storage: '=ngKey'
        },
        transclude: true,
        templateUrl: '/app/views/directives/modal-key-storage.html'
    };
});

// project.directive('ngFileRead', function (StorageService) {
//     return {
//         restrict: "A",
//         link: function (scope, element, attributes) {
//             element.bind("change", function (changeEvent) {
//                 console.log(changeEvent.target.files)
//                 StorageService.UploadKey(changeEvent.target.files);
//             });
//         }
//     }
// });

// project.directive('selectStorage', function()
// {
//     return {
//         link: function(scope, element){
            
//         },
//         scope: {
//             id: '=selectStorage',
//             selected: '=selected'
//             // selected: '@selected'
//         },
//         restrict: 'A'
//     };
// });

project.directive('ngCreateFile', function($window, FileService) {
    function main(scope, element, attrs) {
        scope.isshow = false;
        scope.Upload = function(name, files)
        {
            FileService.Upload(name, files);
            FileService.GetFiles();
        };

        scope.Show = function()
        {
            scope.isshow = !scope.isshow;
        };
    }

    return {
        link: main,
        restrict: "A",
        scope: {

        },
        transclude: true,
        templateUrl: '/app/views/directives/modal-create-file.html',
    };
});

project.directive('modalChangeEmail', function($window) {
    function main(scope, element, attrs) {
        scope.isshow = false;
        // scope.storage = new Storage();
        scope.close = function()
        {
            scope.isshow = !scope.isshow;
        }
    }

    return {
        link: main,
        restrict: "E",
        scope: {
            title: '@title',
            message: '@message',
            isshow: '=isshow'
        },
        templateUrl: '/app/views/directives/modal-change-email.html',

    };
});

project.directive('modalChangePassword', function($window) {
    function main(scope, element, attrs) {
        scope.changePassword = new ChangePasswordModel();
        scope.isshow = false;
        // scope.storage = new Storage();
        scope.close = function()
        {
            scope.isshow = !scope.isshow;
        }
    }

    return {
        link: main,
        restrict: "E",
        scope: {
            title: '@title',
            message: '@message',
            isshow: '=isshow'
        },
        templateUrl: '/app/views/directives/modal-change-password.html',

    };
});

project.directive('ngCopy', function($window, $rootScope, $compile) {
    function main(scope, element, attrs) {
        element.on('click', function(event){
            navigator.clipboard.writeText(event.toElement.innerHTML)
                .then(() => {
                    
                })
                .catch(err => {
                    console.log('Something went wrong', err);
                });
        });
    }

    return {
        link: main,
        restrict: "AE",
        scope: {
            
        }
        // templateUrl: '/app/views/directives/modal-change-password.html',

    };
});

// project.directive('ngDownload', function($window, FileService) {
//     function main(scope, element, attrs) {
//         var active = true;
//         element.on('click', function(event){
//             if(active){
//                 var tempName = Math.random();
//                 element.attr('name', tempName);
//                 if(scope.encrypt)
//                     FileService.GetEncryptFile(scope.name, document.getElementsByName(tempName)[0]);
//                 else
//                     FileService.GetDecryptFile(scope.name, document.getElementsByName(tempName)[0]);
//                 active = false;
//             }
//         });
//     }

//     return {
//         link: main,
//         restrict: "A",
//         scope: {
//             name: '@ngDownload',
//             encrypt: '=ngEncrypt'
//         }
//     };
// });

// project.directive('colorText', function($window) {
//     return {
//         link: function(scope, element, attrs) {
//             var text = angular.element(element);
//             console.log(scope.color)
//             text.css('color', scope.color);
//         },
//         scope: {
//             color: '&colorText'
//         }

//     };
// });