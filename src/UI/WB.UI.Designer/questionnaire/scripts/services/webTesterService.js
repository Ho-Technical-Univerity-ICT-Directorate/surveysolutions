﻿(function() {
    angular.module('designerApp')
        .factory('webTesterService', [
            '$http', function($http) {
                var webTesterService = {};

                webTesterService.run = function(questionnaireId) {
                    return $http.get('../../api/questionnaire/webTest/' + questionnaireId);
                };

                return webTesterService;
            }
        ]);
})();