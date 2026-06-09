import { SimpleMasterDataList } from "@/components/settings/simple-master-data-list";

export default function PayrollGroupsPage() {
  return (
    <SimpleMasterDataList
      objectType="PayrollGroup"
      title="مجموعات الرواتب"
      description="مجموعات صرف الرواتب ودوراتها"
      backHref="/settings/payroll"
      backLabel="إعدادات الرواتب"
    />
  );
}
