'use strict';

const UnsafeRandomModule = require('../random/UnsafeRandomModule.js');
const failureReasons = require('./failure_reasons.json');

class FailureReasonRepository {
    static retrieveRandomReason() {
        return UnsafeRandomModule.randomInArray(failureReasons);
    }
}

module.exports = FailureReasonRepository;