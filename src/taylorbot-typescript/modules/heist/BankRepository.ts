import { EnvUtil } from '../util/EnvUtil';

const banks: { maximumUserCount: number | null; bankName: string; minimumRollForSuccess: number; payoutMultiplier: string }[] = JSON.parse(EnvUtil.getRequiredEnvVariable('TaylorBot_Heist__Banks'));

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
