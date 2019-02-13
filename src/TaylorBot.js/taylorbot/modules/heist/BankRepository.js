'use strict';

const banks = require('./banks.json');

class BankRepository {
    static retrieveBank(userCount) {
        return banks.find(
            bank => bank.maximumUserCount === null || userCount <= bank.maximumUserCount
        );
    }
}

module.exports = BankRepository;