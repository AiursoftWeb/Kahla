import { AiurCollection } from '../AiurCollection';
import { ProbeFile } from '../Probe/ProbeFile';

export interface FileHistoryApiModel extends AiurCollection<ProbeFile> {
    siteName: string;
    rootPath: string;
    showingDateUTC: string;
}
