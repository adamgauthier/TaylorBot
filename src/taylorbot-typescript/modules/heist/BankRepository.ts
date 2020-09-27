import banks = require('./banks.json');

export class BankRepository {
    static retrieveBank(userCount: number): {
        maximumUserCount: number | null;
        bankName: string;
        minimumRollForSuccess: number;
        payoutMultiplier: string;
    } | undefined {
        return banks.find(
            bank => bank.maximumUserCount === null || userCount <= bank.maximumUserCount
        );
    }
}
