import { UnsafeRandomModule } from '../random/UnsafeRandomModule';
import failureReasons = require('./failure_reasons.json');

export class FailureReasonRepository {
    static retrieveRandomReason(): string {
        return UnsafeRandomModule.randomInArray(failureReasons);
    }
}
