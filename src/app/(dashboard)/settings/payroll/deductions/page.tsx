import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function DeductionsPage() {
  return (
    <SimpleMasterDataList
      objectType="DeductionType"
      title="الاستقطاعات"
      description="أنواع الاستقطاعات (تأمينات، سلف، جزاءات)"
      backHref="/settings/payroll"
      backLabel="إعدادات الرواتب"
    />
  );
}
